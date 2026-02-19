import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { ApiService, PaginatedResponse } from '../../../core/api/api.service';
import {
  CreateRecognitionRuleRequest,
  RecognitionRule,
  UpdateRecognitionRuleRequest,
} from '../../../core/models/recognition-rule.model';

export interface PagedResult<T> {
  data: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface RecognitionRuleListRequest {
  page?: number;
  pageSize?: number;
  roleId?: string;
}

export interface ValidationResult {
  isValid: boolean;
  errors: string[];
}

export interface TestResult {
  matches: boolean;
  matchedConditions: string[];
}

export interface ConflictInfo {
  ruleId1: string;
  ruleId2: string;
  roleName1: string;
  roleName2: string;
  description: string;
}

@Injectable({
  providedIn: 'root',
})
export class RulesService {
  private readonly apiService = inject(ApiService);
  private readonly endpoint = '/recognition-rules';

  getRules(
    page: number,
    pageSize: number,
    filters?: RecognitionRuleListRequest,
  ): Observable<PagedResult<RecognitionRule>> {
    const params: Record<string, string | number | boolean> = {
      pageNumber: page,
      pageSize,
    };

    if (filters?.roleId) {
      params['roleId'] = filters.roleId;
    }

    return this.apiService
      .get<PaginatedResponse<RecognitionRule>>(this.endpoint, { params })
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

  getRule(id: string): Observable<RecognitionRule> {
    return this.apiService.get<RecognitionRule>(`${this.endpoint}/${id}`);
  }

  createRule(dto: CreateRecognitionRuleRequest): Observable<string> {
    return this.apiService
      .post<{ id: string }>(this.endpoint, dto)
      .pipe(map(response => response.id));
  }

  updateRule(id: string, dto: UpdateRecognitionRuleRequest): Observable<void> {
    return this.apiService.put<void>(`${this.endpoint}/${id}`, dto);
  }

  deleteRule(id: string): Observable<void> {
    return this.apiService.delete<void>(`${this.endpoint}/${id}`);
  }

  validateRule(formula: string): Observable<boolean> {
    return this.apiService.post<boolean>(`${this.endpoint}/validate`, {
      formula,
    });
  }

  testRule(id: string, familyName: string): Observable<boolean> {
    return this.apiService.post<boolean>(`${this.endpoint}/test`, {
      id,
      familyName,
    });
  }

  checkConflicts(excludeRuleId?: string): Observable<ConflictInfo[]> {
    const payload: Record<string, unknown> = {};

    if (excludeRuleId) {
      payload['excludeId'] = excludeRuleId;
    }

    return this.apiService.post<ConflictInfo[]>(
      `${this.endpoint}/check-conflicts`,
      payload,
    );
  }
}
