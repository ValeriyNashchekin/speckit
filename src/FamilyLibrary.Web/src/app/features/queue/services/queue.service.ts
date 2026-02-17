import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { ApiService, PaginatedResponse } from '../../../core/api/api.service';
import {
  Draft,
  DraftListRequest,
  UpdateDraftRequest,
} from '../../../core/models/draft.model';
import {
  Family,
  FamilyListRequest,
} from '../../../core/models/family.model';

export interface PagedResult<T> {
  data: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface BatchPublishRequest {
  draftIds: string[];
}

export interface BatchPublishResult {
  successful: number;
  failed: number;
  errors: string[];
}

export interface LibraryStatistics {
  totalFamilies: number;
  familiesByRole: Record<string, number>;
  recentPublishes: number;
  pendingDrafts: number;
}

@Injectable({
  providedIn: 'root',
})
export class QueueService {
  private readonly apiService = inject(ApiService);

  // Families endpoints
  getFamilies(
    page: number,
    pageSize: number,
    filters?: FamilyListRequest,
  ): Observable<PagedResult<Family>> {
    const params: Record<string, string | number | boolean> = {
      page,
      pageSize,
    };

    if (filters?.searchTerm) {
      params['searchTerm'] = filters.searchTerm;
    }
    if (filters?.roleId) {
      params['roleId'] = filters.roleId;
    }

    return this.apiService
      .get<PaginatedResponse<Family>>('/families', { params })
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

  // Drafts endpoints
  getDrafts(
    page: number,
    pageSize: number,
    filters?: DraftListRequest,
  ): Observable<PagedResult<Draft>> {
    const params: Record<string, string | number | boolean> = {
      page,
      pageSize,
    };

    if (filters?.status) {
      params['status'] = filters.status;
    }
    if (filters?.searchTerm) {
      params['searchTerm'] = filters.searchTerm;
    }

    return this.apiService
      .get<PaginatedResponse<Draft>>('/drafts', { params })
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

  createDraft(familyFile: File): Observable<Draft> {
    const formData = new FormData();
    formData.append('familyFile', familyFile);
    return this.apiService.post<Draft>('/drafts', formData);
  }

  updateDraft(id: string, dto: UpdateDraftRequest): Observable<Draft> {
    return this.apiService.put<Draft>(`/drafts/${id}`, dto);
  }

  deleteDraft(id: string): Observable<void> {
    return this.apiService.delete<void>(`/drafts/${id}`);
  }

  batchPublish(request: BatchPublishRequest): Observable<BatchPublishResult> {
    return this.apiService.post<BatchPublishResult>('/drafts/batch', request);
  }

  // Library statistics
  getStatistics(): Observable<LibraryStatistics> {
    return this.apiService.get<LibraryStatistics>('/statistics');
  }
}
