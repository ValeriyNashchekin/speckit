import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  input,
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
  LogicalOperator,
  RecognitionCondition,
  RecognitionGroup,
  RecognitionNode,
  RecognitionOperator,
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

  saved = output<CreateRecognitionRuleRequest | UpdateRecognitionRuleRequest>();
  closed = output<void>();

  protected readonly activeTab = signal<'visual' | 'formula'>('visual');
  protected readonly selectedRoleId = signal<string | null>(null);
  protected readonly formula = signal('');
  protected readonly rootNode = signal<RecognitionGroup>({
    type: 'group',
    operator: 'And',
    children: [],
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
    const rolesList = this.roles();
    return rolesList.map(r => ({ label: r.name, value: r.id }));
  });

  private readonly rulesService = inject(RulesService);

  constructor() {
    // Effect to sync rule input to form state (runs once when rule changes)
    effect(() => {
      const rule = this.rule();
      if (rule) {
        this.selectedRoleId.set(rule.roleId);
        this.rootNode.set(JSON.parse(JSON.stringify(rule.rootNode)));
        this.formula.set(rule.formula);
      } else {
        this.resetForm();
      }
    }, { allowSignalWrites: true });

    // Note: Removed the problematic effect that was causing infinite loop.
    // Formula parsing is now handled in onFormulaChange() which is event-driven.
  }

  protected onRootNodeChange(newRoot: RecognitionGroup): void {
    this.rootNode.set(newRoot);
    const newFormula = this.generateFormula(newRoot);
    this.formula.set(newFormula);
    this.checkForConflicts();
  }

  protected onFormulaChange(value: string): void {
    this.formula.set(value);
    try {
      const parsed = this.parseFormula(value);
      if (parsed) {
        this.rootNode.set(parsed);
      }
    } catch {
      // Invalid formula
    }
    this.checkForConflicts();
  }

  protected onRoleChange(roleId: string): void {
    this.selectedRoleId.set(roleId);
    this.checkForConflicts();
  }

  private checkForConflicts(): void {
    const roleId = this.selectedRoleId();
    const root = this.rootNode();
    const excludeRuleId = this.rule()?.id;

    if (!roleId || root.children.length === 0) {
      this.conflicts.set([]);
      return;
    }

    this.isCheckingConflicts.set(true);

    this.rulesService.checkConflicts(roleId, root, excludeRuleId).subscribe({
      next: result => {
        this.conflicts.set(result.conflicts);
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

    const rootNodeValue = this.rootNode();

    if (this.isEditMode()) {
      const updateRequest: UpdateRecognitionRuleRequest = {
        rootNode: rootNodeValue,
      };
      this.saved.emit(updateRequest);
    } else {
      const createRequest: CreateRecognitionRuleRequest = {
        roleId,
        rootNode: rootNodeValue,
      };
      this.saved.emit(createRequest);
    }

    this.isSubmitting.set(false);
  }

  protected onCancel(): void {
    this.closed.emit();
  }

  protected onHide(): void {
    this.closed.emit();
  }

  private resetForm(): void {
    this.selectedRoleId.set(null);
    this.rootNode.set({
      type: 'group',
      operator: 'And',
      children: [],
    });
    this.formula.set('');
    this.conflicts.set([]);
    this.activeTab.set('visual');
  }

  private generateFormula(node: RecognitionNode): string {
    if (isRecognitionCondition(node)) {
      const op = node.operator === 'Contains' ? 'CONTAINS' : 'NOTCONTAINS';
      return op + '("' + node.value + '")';
    }

    if (isRecognitionGroup(node)) {
      const parts = node.children.map(child => this.generateFormula(child));
      const op = node.operator === 'And' ? ' AND ' : ' OR ';
      return '(' + parts.join(op) + ')';
    }

    return '';
  }

  private parseFormula(formula: string): RecognitionGroup | null {
    if (!formula.trim()) {
      return {
        type: 'group',
        operator: 'And',
        children: [],
      };
    }

    try {
      return this.parseExpression(formula.trim());
    } catch {
      return null;
    }
  }

  private parseExpression(expr: string): RecognitionGroup {
    expr = expr.trim();

    if (expr.startsWith('(') && expr.endsWith(')')) {
      let depth = 0;
      let allWrapped = true;
      for (let i = 0; i < expr.length - 1; i++) {
        if (expr[i] === '(') depth++;
        if (expr[i] === ')') depth--;
        if (depth === 0) {
          allWrapped = false;
          break;
        }
      }
      if (allWrapped) {
        expr = expr.slice(1, -1).trim();
      }
    }

    let depth = 0;
    const andIndices: number[] = [];
    const orIndices: number[] = [];

    for (let i = 0; i < expr.length; i++) {
      if (expr[i] === '(') depth++;
      if (expr[i] === ')') depth--;
      
      if (depth === 0) {
        if (expr.substring(i, i + 5) === ' AND ') {
          andIndices.push(i);
        }
        if (expr.substring(i, i + 4) === ' OR ') {
          orIndices.push(i);
        }
      }
    }

    let operator: LogicalOperator = 'And';
    let splitIndices: number[] = [];

    if (orIndices.length > 0) {
      operator = 'Or';
      splitIndices = orIndices;
    } else if (andIndices.length > 0) {
      operator = 'And';
      splitIndices = andIndices;
    }

    if (splitIndices.length === 0) {
      if (expr.startsWith('CONTAINS(') || expr.startsWith('NOTCONTAINS(')) {
        const condition = this.parseCondition(expr);
        return {
          type: 'group',
          operator: 'And',
          children: [condition],
        };
      }
      
      return this.parseExpression(expr);
    }

    const children: RecognitionNode[] = [];
    let lastIndex = 0;

    for (const index of [...splitIndices, expr.length]) {
      const part = expr.substring(lastIndex, index).trim();
      if (part) {
        if (part.startsWith('CONTAINS(') || part.startsWith('NOTCONTAINS(')) {
          children.push(this.parseCondition(part));
        } else {
          children.push(this.parseExpression(part));
        }
      }
      lastIndex = index + (operator === 'And' ? 5 : 4);
    }

    return {
      type: 'group',
      operator,
      children,
    };
  }
  private parseCondition(expr: string): RecognitionCondition {
    const containsRegex = new RegExp('CONTAINS\\("(.+?)"\\)');
    const notContainsRegex = new RegExp('NOTCONTAINS\\("(.+?)"\\)');

    const containsMatch = expr.match(containsRegex);
    const notContainsMatch = expr.match(notContainsRegex);

    if (containsMatch) {
      return {
        type: 'condition',
        operator: 'Contains' as RecognitionOperator,
        value: containsMatch[1],
      };
    }

    if (notContainsMatch) {
      return {
        type: 'condition',
        operator: 'NotContains' as RecognitionOperator,
        value: notContainsMatch[1],
      };
    }

    throw new Error('Invalid condition: ' + expr);
  }

  protected get canSave(): boolean {
    const roleId = this.selectedRoleId();
    const root = this.rootNode();
    return !!roleId && root.children.length > 0;
  }
}
