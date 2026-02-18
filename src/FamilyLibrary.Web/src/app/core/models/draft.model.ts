// DTOs matching backend Draft entity

export type DraftStatus = 'New' | 'RoleSelected' | 'Stamped' | 'Published';

export interface Draft {
  id: string;
  familyName: string;
  familyUniqueId: string;
  selectedRoleId: string | null;
  templateId: string | null;
  status: DraftStatus;
  createdAt: string;
  lastSeen: string;
}

export interface UpdateDraftRequest {
  selectedRoleId?: string | null;
}

export interface DraftListRequest {
  page?: number;
  pageSize?: number;
  status?: DraftStatus | null;
  searchTerm?: string;
}
