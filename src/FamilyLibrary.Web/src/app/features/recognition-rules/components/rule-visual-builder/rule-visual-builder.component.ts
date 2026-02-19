import { NgTemplateOutlet } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
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
  path: number[];
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
  rootNode = input.required<RecognitionGroup>();
  rootNodeChange = output<RecognitionGroup>();

  protected readonly selectedNodeKey = signal<string | null>(null);

  protected readonly operatorOptions: Array<{ label: string; value: RecognitionOperator }> = [
    { label: 'Contains', value: 'Contains' },
    { label: 'Not Contains', value: 'NotContains' },
  ];

  protected readonly logicalOperatorOptions: Array<{ label: string; value: LogicalOperator }> = [
    { label: 'AND', value: 'And' },
    { label: 'OR', value: 'Or' },
  ];

  private readonly collapsedKeys = signal<Set<string>>(new Set());

  protected readonly treeNodes = computed(() => {
    const root = this.rootNode();
    const collapsed = this.collapsedKeys();

    const convert = (node: RecognitionNode, parentKey: string, path: number[]): TreeNode[] => {
      const key = path.length === 0 ? 'root' : `${parentKey}-${path[path.length - 1]}`;

      if (isRecognitionCondition(node)) {
        const op = node.operator === 'Contains' ? 'contains' : 'not contains';
        return [{
          key,
          path,
          label: `${op} "${node.value || '...'}"`,
          data: node,
          expanded: false,
        }];
      }

      if (isRecognitionGroup(node)) {
        const treeNode: TreeNode = {
          key,
          path,
          label: node.operator.toUpperCase(),
          data: node,
          children: node.children.flatMap((child, i) => convert(child, key, [...path, i])),
          expanded: !collapsed.has(key),
        };
        return [treeNode];
      }

      return [];
    };

    return convert(root, '', []);
  });

  protected readonly selectedNode = computed(() => {
    const key = this.selectedNodeKey();
    if (!key) return null;
    return this.findNodeByKey(this.treeNodes(), key);
  });

  protected readonly selectedCondition = computed(() => {
    const node = this.selectedNode();
    return node && isRecognitionCondition(node.data) ? node.data : null;
  });

  protected readonly selectedGroup = computed(() => {
    const node = this.selectedNode();
    return node && isRecognitionGroup(node.data) ? node.data : null;
  });

  protected selectNode(key: string): void {
    this.selectedNodeKey.set(key);
  }

  protected addCondition(): void {
    const selected = this.selectedNode();
    if (!selected || !isRecognitionGroup(selected.data)) return;

    const root = this.cloneRoot();
    const target = this.getNodeAtPath(root, selected.path) as RecognitionGroup;
    target.children.push({ type: 'condition', operator: 'Contains', value: '' });
    this.rootNodeChange.emit(root);
  }

  protected addGroup(): void {
    const selected = this.selectedNode();
    if (!selected || !isRecognitionGroup(selected.data)) return;

    const root = this.cloneRoot();
    const target = this.getNodeAtPath(root, selected.path) as RecognitionGroup;
    target.children.push({ type: 'group', operator: 'And', children: [] });
    this.rootNodeChange.emit(root);
  }

  protected removeNode(key: string): void {
    const node = this.findNodeByKey(this.treeNodes(), key);
    if (!node || node.path.length === 0) return;

    const root = this.cloneRoot();
    const parentPath = node.path.slice(0, -1);
    const childIndex = node.path[node.path.length - 1];
    const parent = this.getNodeAtPath(root, parentPath) as RecognitionGroup;
    parent.children.splice(childIndex, 1);

    this.selectedNodeKey.set(null);
    this.rootNodeChange.emit(root);
  }

  protected updateConditionValue(value: string): void {
    const selected = this.selectedNode();
    if (!selected || !isRecognitionCondition(selected.data)) return;

    const root = this.cloneRoot();
    const condition = this.getNodeAtPath(root, selected.path) as RecognitionCondition;
    condition.value = value;
    this.rootNodeChange.emit(root);
  }

  protected updateConditionOperator(operator: RecognitionOperator): void {
    const selected = this.selectedNode();
    if (!selected || !isRecognitionCondition(selected.data)) return;

    const root = this.cloneRoot();
    const condition = this.getNodeAtPath(root, selected.path) as RecognitionCondition;
    condition.operator = operator;
    this.rootNodeChange.emit(root);
  }

  protected updateGroupOperator(operator: LogicalOperator): void {
    const selected = this.selectedNode();
    if (!selected || !isRecognitionGroup(selected.data)) return;

    const root = this.cloneRoot();
    const group = this.getNodeAtPath(root, selected.path) as RecognitionGroup;
    group.operator = operator;
    this.rootNodeChange.emit(root);
  }

  protected toggleNode(key: string, event: Event): void {
    event.stopPropagation();
    this.collapsedKeys.update(keys => {
      const next = new Set(keys);
      if (next.has(key)) {
        next.delete(key);
      } else {
        next.add(key);
      }
      return next;
    });
  }

  protected isGroup(node: TreeNode | null): boolean {
    return node !== null && isRecognitionGroup(node.data);
  }

  protected isCondition(node: TreeNode | null): boolean {
    return node !== null && isRecognitionCondition(node.data);
  }

  private cloneRoot(): RecognitionGroup {
    return JSON.parse(JSON.stringify(this.rootNode()));
  }

  private getNodeAtPath(root: RecognitionNode, path: number[]): RecognitionNode {
    let current = root;
    for (const index of path) {
      current = (current as RecognitionGroup).children[index];
    }
    return current;
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
}
