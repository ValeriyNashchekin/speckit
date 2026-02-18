import { NgTemplateOutlet } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  input,
  output,
  signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import {
  isRecognitionCondition,
  isRecognitionGroup,
  LogicalOperator,
  RecognitionCondition,
  RecognitionGroup,
  RecognitionNode,
  RecognitionOperator,
} from '../../../../core/models/recognition-rule.model';

interface TreeNode {
  key: string;
  label: string;
  data: RecognitionNode;
  children?: TreeNode[];
  expanded: boolean;
}

@Component({
  selector: 'app-rule-visual-builder',
  imports: [ButtonModule, FormsModule, InputTextModule, NgTemplateOutlet, SelectModule],
  templateUrl: './rule-visual-builder.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RuleVisualBuilderComponent {
  // inputs
  rootNode = input.required<RecognitionGroup>();

  // outputs
  rootNodeChange = output<RecognitionGroup>();

  // state
  protected readonly treeNodes = signal<TreeNode[]>([]);
  protected readonly selectedNodeKey = signal<string | null>(null);
  protected readonly nextId = signal(0);

  // options for dropdowns
  protected readonly operatorOptions = signal<
    Array<{ label: string; value: RecognitionOperator }>
  >([
    { label: 'Contains', value: 'Contains' },
    { label: 'Not Contains', value: 'NotContains' },
  ]);

  protected readonly logicalOperatorOptions = signal<
    Array<{ label: string; value: LogicalOperator }>
  >([
    { label: 'AND', value: 'And' },
    { label: 'OR', value: 'Or' },
  ]);

  // computed
  protected readonly selectedNode = computed(() => {
    const key = this.selectedNodeKey();
    if (!key) return null;
    return this.findNodeByKey(this.treeNodes(), key);
  });

  protected readonly selectedCondition = computed(() => {
    const node = this.selectedNode();
    if (node && isRecognitionCondition(node.data)) {
      return node.data;
    }
    return null;
  });

  protected readonly selectedGroup = computed(() => {
    const node = this.selectedNode();
    if (node && isRecognitionGroup(node.data)) {
      return node.data;
    }
    return null;
  });

  constructor() {
    // Sync tree when rootNode changes
    // Reset ID counter before conversion to ensure stable keys
    effect(() => {
      const root = this.rootNode();
      // Reset the counter for consistent key generation
      this.nextId.set(0);
      const nodes = this.convertToTreeNodes(root, 'root');
      this.treeNodes.set(nodes);
    }, { allowSignalWrites: true });
  }

  protected selectNode(key: string): void {
    this.selectedNodeKey.set(key);
  }

  protected addCondition(): void {
    const selected = this.selectedNode();
    if (!selected) return;

    const newCondition: RecognitionCondition = {
      type: 'condition',
      operator: 'Contains',
      value: '',
    };

    if (isRecognitionGroup(selected.data)) {
      selected.data.children.push(newCondition);
      this.syncToRoot();
    }
  }

  protected addGroup(): void {
    const selected = this.selectedNode();
    if (!selected) return;

    const newGroup: RecognitionGroup = {
      type: 'group',
      operator: 'And',
      children: [],
    };

    if (isRecognitionGroup(selected.data)) {
      selected.data.children.push(newGroup);
      this.syncToRoot();
    }
  }

  protected removeNode(key: string): void {
    const parent = this.findParentNode(this.treeNodes()[0], key);
    if (parent && isRecognitionGroup(parent.data)) {
      const nodeToRemove = this.findNodeByKey(this.treeNodes(), key);
      if (nodeToRemove) {
        const index = parent.data.children.indexOf(nodeToRemove.data);
        if (index > -1) {
          parent.data.children.splice(index, 1);
          this.syncToRoot();
        }
      }
    }
    this.selectedNodeKey.set(null);
  }

  protected updateConditionValue(value: string): void {
    const condition = this.selectedCondition();
    if (condition) {
      condition.value = value;
      this.syncToRoot();
    }
  }

  protected updateConditionOperator(operator: RecognitionOperator): void {
    const condition = this.selectedCondition();
    if (condition) {
      condition.operator = operator;
      this.syncToRoot();
    }
  }

  protected updateGroupOperator(operator: LogicalOperator): void {
    const group = this.selectedGroup();
    if (group) {
      group.operator = operator;
      this.syncToRoot();
    }
  }

  protected toggleNode(key: string): void {
    this.toggleNodeExpanded(this.treeNodes(), key);
    this.treeNodes.update(nodes => [...nodes]);
  }

  private toggleNodeExpanded(nodes: TreeNode[], key: string): boolean {
    for (const node of nodes) {
      if (node.key === key) {
        node.expanded = !node.expanded;
        return true;
      }
      if (node.children && this.toggleNodeExpanded(node.children, key)) {
        return true;
      }
    }
    return false;
  }

  private syncToRoot(): void {
    const root = this.treeNodes()[0]?.data as RecognitionGroup;
    if (root) {
      this.rootNodeChange.emit(root);
    }
  }

  private convertToTreeNodes(node: RecognitionNode, parentKey: string): TreeNode[] {
    const id = this.nextId();
    this.nextId.update(n => n + 1);
    const key = `${parentKey}-${id}`;

    if (isRecognitionCondition(node)) {
      return [
        {
          key,
          label: this.getConditionLabel(node),
          data: node,
          expanded: false,
        },
      ];
    }

    if (isRecognitionGroup(node)) {
      const treeNode: TreeNode = {
        key,
        label: this.getGroupLabel(node),
        data: node,
        children: node.children.flatMap(child =>
          this.convertToTreeNodes(child, key),
        ),
        expanded: true,
      };
      return [treeNode];
    }

    return [];
  }

  private getConditionLabel(condition: RecognitionCondition): string {
    const op = condition.operator === 'Contains' ? 'contains' : 'not contains';
    return `${op} "${condition.value || '...'}"`;
  }

  private getGroupLabel(group: RecognitionGroup): string {
    return group.operator.toUpperCase();
  }

  private findNodeByKey(nodes: TreeNode[], key: string): TreeNode | null {
    for (const node of nodes) {
      if (node.key === key) return node;
      if (node.children) {
        const found = this.findNodeByKey(node.children, key);
        if (found) return found;
      }
    }
    return null;
  }

  private findParentNode(
    node: TreeNode | null,
    childKey: string,
  ): TreeNode | null {
    if (!node) return null;

    if (node.children) {
      for (const child of node.children) {
        if (child.key === childKey) return node;
        const found = this.findParentNode(child, childKey);
        if (found) return found;
      }
    }
    return null;
  }

  protected isGroup(node: TreeNode | null): boolean {
    return node !== null && isRecognitionGroup(node.data);
  }

  protected isCondition(node: TreeNode | null): boolean {
    return node !== null && isRecognitionCondition(node.data);
  }
}
