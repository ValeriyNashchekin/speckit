// DTOs matching backend Family and FamilyVersion entities

export interface Family {
  id: string;
  roleId: string;
  familyName: string;
  currentVersion: number;
  createdAt: string;
  updatedAt: string;
}

export interface FamilyVersion {
  id: string;
  familyId: string;
  version: number;
  hash: string;
  previousHash: string | null;
  blobPath: string;
  catalogBlobPath: string | null;
  originalFileName: string;
  originalCatalogName: string | null;
  commitMessage: string | null;
  snapshotJson: string;
  publishedAt: string;
  publishedBy: string;
}

export interface FamilyDetail extends Family {
  versions: FamilyVersion[];
}

export interface FamilyListRequest {
  page?: number;
  pageSize?: number;
  searchTerm?: string;
  roleId?: string | null;
}

export interface PublishFamilyRequest {
  familyFile: File;
  catalogFile?: File | null;
  roleId: string;
  commitMessage?: string | null;
}

export interface PublishResult {
  familyId: string;
  version: number;
  message: string;
}
