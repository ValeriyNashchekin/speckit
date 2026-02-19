import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  input,
  linkedSignal,
  output,
  signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { MessageModule } from 'primeng/message';
import { SelectModule } from 'primeng/select';
import { TextareaModule } from 'primeng/textarea';
import {
  CreateRecognitionRuleRequest,
  isRecognitionCondition,
  isRecognitionGroup,
  RecognitionCondition,
  RecognitionGroup,
  RecognitionNode,
  RecognitionRule,
  UpdateRecognitionRuleRequest,
} from '../../../../core/models/recognition-rule.model';
import { ConflictInfo, RulesService } from '../../services/rules.service';
import { RuleVisualBuilderComponent } from '../rule-visual-builder/rule-visual-builder.component';

@Component({
  selector: 'app-rule-editor',
  imports: [
    ButtonModule,
    DialogModule,
    FormsModule,
    MessageModule,
    SelectModule,
    TextareaModule,
    RuleVisualBuilderComponent,
  ],
  templateUrl: './rule-editor.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RuleEditorComponent {
  visible = input<boolean>(false);
  rule = input<RecognitionRule | null>(null);
  roles = input<Array<{ id: string; name: string }>>([]);

  saved = output<void>();
  closed = output<void>();

  protected readonly activeTab = linkedSignal<RecognitionRule | null, 'visual' | 'formula'>({
    source: this.rule,
    computation: () => 'visual',
  });

  protected readonly selectedRoleId = linkedSignal<RecognitionRule | null, string | null>({
    source: this.rule,
    computation: rule => rule?.roleId ?? null,
  });

  protected readonly rootNode = linkedSignal<RecognitionRule | null, RecognitionGroup>({
    source: this.rule,
    computation: rule => {
      if (rule?.rootNode) {
        const parsed = typeof rule.rootNode === 'string'
          ? JSON.parse(rule.rootNode as string)
          : rule.rootNode;
        return JSON.parse(JSON.stringify(parsed));
      }
      return { type: 'group', operator: 'And', children: [] };
    },
  });

  protected readonly formula = linkedSignal<RecognitionRule | null, string>({
    source: this.rule,
    computation: rule => rule?.formula ?? '',
  });

  protected readonly isSubmitting = signal(false);
  protected readonly isCheckingConflicts = signal(false);
  protected readonly conflicts = signal<ConflictInfo[]>([]);

  protected readonly isEditMode = computed(() => this.rule() !== null);
  protected readonly dialogTitle = computed(() =>
    this.isEditMode() ? 'Edit Recognition Rule' : 'New Recognition Rule',
  );

  protected readonly hasConflicts = computed(() => this.conflicts().length > 0);

  protected readonly roleOptions = computed(() => {
    return this.roles().map(r => ({ label: r.name, value: r.id }));
  });

  protected readonly canSave = computed(() => {
    const roleId = this.selectedRoleId();
    const root = this.rootNode();
    return !!roleId && root.children.length > 0;
  });

  private readonly rulesService = inject(RulesService);

  protected onRootNodeChange(newRoot: RecognitionGroup): void {
    this.rootNode.set(newRoot);
    this.formula.set(this.nodeToFormula(newRoot));
  }

  protected onFormulaChange(value: string): void {
    this.formula.set(value);
    try {
      const parsed = this.parseFormula(value);
      if (parsed) {
        this.rootNode.set(parsed);
      }
    } catch {
      // keep current rootNode on parse error
    }
  }

  protected onRoleChange(roleId: string): void {
    this.selectedRoleId.set(roleId);
  }

  protected onCheckConflicts(): void {
    const excludeRuleId = this.rule()?.id;
    this.isCheckingConflicts.set(true);

    this.rulesService.checkConflicts(excludeRuleId).subscribe({
      next: result => {
        this.conflicts.set(result);
        this.isCheckingConflicts.set(false);
      },
      error: () => {
        this.conflicts.set([]);
        this.isCheckingConflicts.set(false);
      },
    });
  }

  protected onSave(): void {
    const roleId = this.selectedRoleId();
    if (!roleId) return;

    this.isSubmitting.set(true);

    const rootNodeJson = JSON.stringify(this.rootNode());
    const formulaValue = this.formula();
    const existingRule = this.rule();

    if (this.isEditMode() && existingRule) {
      const request: UpdateRecognitionRuleRequest = {
        rootNode: rootNodeJson,
        formula: formulaValue,
      };
      this.rulesService.updateRule(existingRule.id, request).subscribe({
        next: () => {
          this.isSubmitting.set(false);
          this.saved.emit();
        },
        error: () => {
          this.isSubmitting.set(false);
        },
      });
    } else {
      const request: CreateRecognitionRuleRequest = {
        roleId,
        rootNode: rootNodeJson,
        formula: formulaValue,
      };
      this.rulesService.createRule(request).subscribe({
        next: () => {
          this.isSubmitting.set(false);
          this.saved.emit();
        },
        error: () => {
          this.isSubmitting.set(false);
        },
      });
    }
  }

  protected onCancel(): void {
    this.closed.emit();
  }

  protected onHide(): void {
    this.closed.emit();
  }

  private nodeToFormula(node: RecognitionNode): string {
    if (isRecognitionCondition(node)) {
      if (node.operator === 'NotContains') {
        return `NOT ${node.value}`;
      }
      return node.value;
    }

    if (isRecognitionGroup(node)) {
      if (node.children.length === 0) return '';
      if (node.children.length === 1) return this.nodeToFormula(node.children[0]);

      const separator = node.operator === 'And' ? ' AND ' : ' OR ';
      const parts = node.children.map(child => {
        const childFormula = this.nodeToFormula(child);
        if (isRecognitionGroup(child) && child.children.length > 1) {
          return `(${childFormula})`;
        }
        return childFormula;
      });
      return parts.join(separator);
    }

    return '';
  }

  private parseFormula(formula: string): RecognitionGroup | null {
    const trimmed = formula.trim();
    if (!trimmed) {
      return { type: 'group', operator: 'And', children: [] };
    }

    const tokens = this.tokenize(trimmed);
    if (tokens.length === 0) {
      return { type: 'group', operator: 'And', children: [] };
    }

    let pos = 0;

    const parseOr = (): RecognitionNode => {
      const children: RecognitionNode[] = [parseAnd()];
      while (pos < tokens.length && tokens[pos].toUpperCase() === 'OR') {
        pos++;
        children.push(parseAnd());
      }
      if (children.length === 1) return children[0];
      return { type: 'group', operator: 'Or', children };
    };

    const parseAnd = (): RecognitionNode => {
      const children: RecognitionNode[] = [parsePrimary()];
      while (pos < tokens.length && tokens[pos].toUpperCase() === 'AND') {
        pos++;
        children.push(parsePrimary());
      }
      if (children.length === 1) return children[0];
      return { type: 'group', operator: 'And', children };
    };

    const parsePrimary = (): RecognitionNode => {
      if (pos >= tokens.length) {
        return { type: 'condition', operator: 'Contains', value: '' } as RecognitionCondition;
      }

      const token = tokens[pos];

      if (token === '(') {
        pos++;
        const node = parseOr();
        if (pos < tokens.length && tokens[pos] === ')') {
          pos++;
        }
        return node;
      }

      if (token.toUpperCase() === 'NOT') {
        pos++;
        if (pos < tokens.length && tokens[pos] !== '(' && tokens[pos].toUpperCase() !== 'AND' && tokens[pos].toUpperCase() !== 'OR') {
          const value = tokens[pos];
          pos++;
          return { type: 'condition', operator: 'NotContains', value } as RecognitionCondition;
        }
        return { type: 'condition', operator: 'NotContains', value: '' } as RecognitionCondition;
      }

      pos++;
      return { type: 'condition', operator: 'Contains', value: token } as RecognitionCondition;
    };

    const result = parseOr();

    if (isRecognitionGroup(result)) {
      return result;
    }
    return { type: 'group', operator: 'And', children: [result] };
  }

  private tokenize(formula: string): string[] {
    const tokens: string[] = [];
    let i = 0;
    while (i < formula.length) {
      while (i < formula.length && formula[i] === ' ') i++;
      if (i >= formula.length) break;

      if (formula[i] === '(' || formula[i] === ')') {
        tokens.push(formula[i]);
        i++;
        continue;
      }

      let word = '';
      while (i < formula.length && formula[i] !== ' ' && formula[i] !== '(' && formula[i] !== ')') {
        word += formula[i];
        i++;
      }
      if (word) tokens.push(word);
    }
    return tokens;
  }
}
