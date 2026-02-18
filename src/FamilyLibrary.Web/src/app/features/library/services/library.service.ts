import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { ApiService, PaginatedResponse } from '../../../core/api/api.service';
import {
  Family,
  FamilyDetail,
  FamilyListRequest,
  FamilyVersion,
} from '../../../core/models/family.model';

export interface PagedResult<T> {
  data: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root',
})
export class LibraryService {
  private readonly apiService = inject(ApiService);

  /**
   * Get families with search, filters, and pagination.
   */
  getFamilies(
    page: number,
    pageSize: number,
    filters?: FamilyListRequest,
  ): Observable<PagedResult<Family>> {
    const params: Record<string, string | number | boolean> = {
      pageNumber: page,
      pageSize,
    };

    if (filters?.searchTerm) {
      params['searchTerm'] = filters.searchTerm;
    }
    if (filters?.roleId) {
      params['roleId'] = filters.roleId;
    }
    if (filters?.categoryId) {
      params['categoryId'] = filters.categoryId;
    }
    if (filters?.tagIds?.length) {
      params['tagIds'] = filters.tagIds.join(',');
    }

    return this.apiService
      .get<PaginatedResponse<Family>>('/families', { params })
      .pipe(
        map(response => ({
          data: response.items,
          page: response.pageNumber,
          pageSize: response.pageSize,
          totalCount: response.totalCount,
          totalPages: response.totalPages,
        })),
      );
  }

  /**
   * Get family by ID with full details.
   */
  getFamilyById(id: string): Observable<FamilyDetail> {
    return this.apiService.get<FamilyDetail>(`/families/${id}`);
  }

  /**
   * Get version history for a family.
   */
  getFamilyVersions(id: string): Observable<FamilyVersion[]> {
    return this.apiService.get<FamilyVersion[]>(`/families/${id}/versions`);
  }
}
