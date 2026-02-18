// DTOs matching backend RecognitionRule entity

export type RecognitionOperator = 'Contains' | 'NotContains';
export type LogicalOperator = 'And' | 'Or';

export interface RecognitionCondition {
  type: 'condition';
  operator: RecognitionOperator;
  value: string;
}

export interface RecognitionGroup {
  type: 'group';
  operator: LogicalOperator;
  children: RecognitionNode[];
}

export type RecognitionNode = RecognitionCondition | RecognitionGroup;

export interface RecognitionRule {
  id: string;
  roleId: string;
  rootNode: RecognitionGroup;
  formula: string;
}

export interface CreateRecognitionRuleRequest {
  roleId: string;
  rootNode: RecognitionGroup;
}

export interface UpdateRecognitionRuleRequest {
  rootNode: RecognitionGroup;
}

export function isRecognitionCondition(node: RecognitionNode): node is RecognitionCondition {
  return node.type === 'condition';
}

export function isRecognitionGroup(node: RecognitionNode): node is RecognitionGroup {
  return node.type === 'group';
}
