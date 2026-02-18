/**
 * Material mapping model for template-to-project material conversions.
 * Used during Pull Update when template materials don't exist in project.
 */

export interface MaterialMapping {
  id: string;
  projectId: string;
  templateMaterialName: string;
  projectMaterialName: string;
  category?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateMaterialMappingRequest {
  projectId: string;
  templateMaterialName: string;
  projectMaterialName: string;
  category?: string;
}

export interface UpdateMaterialMappingRequest {
  projectMaterialName: string;
  category?: string;
}

export interface MaterialMappingFilter {
  projectId?: string;
  category?: string;
  search?: string;
}
