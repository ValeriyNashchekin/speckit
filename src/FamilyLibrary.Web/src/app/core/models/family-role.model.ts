// DTOs matching backend FamilyRole entity

import { Tag } from './tag.model';

export type RoleType = 'Loadable' | 'System';

export interface FamilyRole {
  id: string;
  name: string;
  type: RoleType;
  description: string | null;
  categoryId: string | null;
  tags?: Tag[];
  createdAt: string;
  updatedAt: string;
}

export interface CreateFamilyRoleRequest {
  name: string;
  type: RoleType;
  description?: string | null;
  categoryId?: string | null;
  tagIds?: string[];
}

export interface UpdateFamilyRoleRequest {
  description?: string | null;
  categoryId?: string | null;
  tagIds?: string[];
}

export interface FamilyRoleListRequest {
  page?: number;
  pageSize?: number;
  searchTerm?: string;
  categoryId?: string | null;
  type?: RoleType | null;
}

export interface ImportPreviewItem {
  name: string;
  type: RoleType;
  description: string | null;
  categoryId: string | null;
  isValid: boolean;
  validationMessage: string | null;
  isDuplicate: boolean;
}

export interface ImportPreviewResponse {
  items: ImportPreviewItem[];
  totalValid: number;
  totalInvalid: number;
  totalDuplicates: number;
}

export interface ImportResultResponse {
  imported: number;
  skipped: number;
  errors: string[];
}
