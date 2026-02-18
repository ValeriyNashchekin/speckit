import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { ApiService, PaginatedResponse } from '../../../core/api/api.service';
import {
  CreateFamilyRoleRequest,
  FamilyRole,
  FamilyRoleListRequest,
  ImportResultResponse,
  UpdateFamilyRoleRequest,
} from '../../../core/models/family-role.model';

export interface PagedResult<T> {
  data: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface BatchCreateResult {
  imported: number;
  skipped: number;
  errors: string[];
}

@Injectable({
  providedIn: 'root',
})
export class RolesService {
  private readonly apiService = inject(ApiService);

  getRoles(
    page: number,
    pageSize: number,
    filters?: FamilyRoleListRequest,
  ): Observable<PagedResult<FamilyRole>> {
    const params: Record<string, string | number | boolean> = {
      page,
      pageSize,
    };

    if (filters?.searchTerm) {
      params['searchTerm'] = filters.searchTerm;
    }
    if (filters?.categoryId) {
      params['categoryId'] = filters.categoryId;
    }
    if (filters?.type) {
      params['type'] = filters.type;
    }

    return this.apiService
      .get<PaginatedResponse<FamilyRole>>('/roles', { params })
      .pipe(
        map(response => ({
          data: response.data,
          page: response.page,
          pageSize: response.pageSize,
          totalCount: response.totalCount,
          totalPages: response.totalPages,
        })),
      );
  }

  getRole(id: string): Observable<FamilyRole> {
    return this.apiService.get<FamilyRole>(`/roles/${id}`);
  }

  createRole(dto: CreateFamilyRoleRequest): Observable<string> {
    return this.apiService.post<{ id: string }>('/roles', dto).pipe(
      map(response => response.id),
    );
  }

  updateRole(id: string, dto: UpdateFamilyRoleRequest): Observable<void> {
    return this.apiService.put<void>(`/roles/${id}`, dto);
  }

  deleteRole(id: string): Observable<void> {
    return this.apiService.delete<void>(`/roles/${id}`);
  }

  importRoles(dtos: CreateFamilyRoleRequest[]): Observable<BatchCreateResult> {
    return this.apiService
      .post<ImportResultResponse>('/roles/import', { items: dtos })
      .pipe(
        map(response => ({
          imported: response.imported,
          skipped: response.skipped,
          errors: response.errors,
        })),
      );
  }
}
