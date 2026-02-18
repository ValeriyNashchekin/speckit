import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../api/api.service';
import {
  MaterialMapping,
  CreateMaterialMappingRequest,
  UpdateMaterialMappingRequest,
  MaterialMappingFilter,
} from '../models/material-mapping.model';

/**
 * Service for managing material mappings between template and project materials.
 * Used during Pull Update workflow when template materials need project equivalents.
 */
@Injectable({ providedIn: 'root' })
export class MaterialMappingService {
  private readonly apiService = inject(ApiService);

  /**
   * Get all material mappings, optionally filtered by project
   */
  getMappings(projectId?: string): Observable<MaterialMapping[]> {
    const params = projectId ? { projectId } : undefined;
    return this.apiService.get<MaterialMapping[]>('/material-mappings', { params });
  }

  /**
   * Get a single material mapping by ID
   */
  getMapping(id: string): Observable<MaterialMapping> {
    return this.apiService.get<MaterialMapping>(`/material-mappings/${id}`);
  }

  /**
   * Get mappings filtered by criteria
   */
  getFilteredMappings(filter: MaterialMappingFilter): Observable<MaterialMapping[]> {
    return this.apiService.get<MaterialMapping[]>('/material-mappings', { params: filter as Record<string, string> });
  }

  /**
   * Create a new material mapping
   */
  createMapping(dto: CreateMaterialMappingRequest): Observable<MaterialMapping> {
    return this.apiService.post<MaterialMapping>('/material-mappings', dto);
  }

  /**
   * Update an existing material mapping
   */
  updateMapping(id: string, dto: UpdateMaterialMappingRequest): Observable<MaterialMapping> {
    return this.apiService.put<MaterialMapping>(`/material-mappings/${id}`, dto);
  }

  /**
   * Delete a material mapping
   */
  deleteMapping(id: string): Observable<void> {
    return this.apiService.delete<void>(`/material-mappings/${id}`);
  }

  /**
   * Find mapping for a specific template material in a project
   */
  findMapping(projectId: string, templateMaterialName: string): Observable<MaterialMapping | null> {
    return this.apiService.get<MaterialMapping | null>(
      `/material-mappings/lookup`,
      { params: { projectId, templateMaterialName } }
    );
  }
}
