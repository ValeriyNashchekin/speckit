// DTOs matching backend FamilyRole entity

export type RoleType = 'Loadable' | 'System';

export interface FamilyRole {
  id: string;
  name: string;
  type: RoleType;
  description: string | null;
  categoryId: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CreateFamilyRoleRequest {
  name: string;
  type: RoleType;
  description?: string | null;
  categoryId?: string | null;
}

export interface UpdateFamilyRoleRequest {
  description?: string | null;
  categoryId?: string | null;
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
