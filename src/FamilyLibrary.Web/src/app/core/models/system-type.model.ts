// DTOs matching backend SystemType entity

export type SystemFamilyGroup = 'GroupA' | 'GroupB' | 'GroupC' | 'GroupD' | 'GroupE';

export interface SystemType {
  id: string;
  roleId: string;
  typeName: string;
  category: string;
  systemFamily: string;
  group: SystemFamilyGroup;
  json: string;
  currentVersion: number;
  hash: string;
  createdAt: string;
  updatedAt: string;
}

export interface SystemTypeListRequest {
  page?: number;
  pageSize?: number;
  searchTerm?: string;
  roleId?: string | null;
  category?: string | null;
}
