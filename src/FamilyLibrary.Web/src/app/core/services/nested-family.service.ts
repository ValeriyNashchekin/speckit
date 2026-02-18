import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../api/api.service';

/**
 * Nested family information from Revit plugin
 */
export interface NestedFamilyInfo {
  familyName: string;
  isShared: boolean;
  hasRole: boolean;
  roleName?: string;
  inLibrary: boolean;
  libraryVersion?: number;
  status: 'ready' | 'not_published' | 'no_role';
}

/**
 * Dependencies response from API
 */
export interface FamilyDependencies {
  parentFamilyId: string;
  parentFamilyName: string;
  nestedFamilies: NestedFamilyInfo[];
}

/**
 * Pre-load summary nested family info
 */
export interface NestedLoadInfo {
  familyName: string;
  roleName?: string;
  rfaVersion: number;
  libraryVersion?: number;
  projectVersion?: number;
  recommendedAction: 'load_from_rfa' | 'update_from_library' | 'keep_project' | 'no_action';
  hasConflict: boolean;
}

/**
 * Pre-load summary response from API
 */
export interface PreLoadSummary {
  parentFamily: {
    familyId: string;
    familyName: string;
    roleName: string;
    version: number;
  };
  nestedFamilies: NestedLoadInfo[];
  summary: {
    totalToLoad: number;
    newCount: number;
    updateCount: number;
    conflictCount: number;
  };
}

/**
 * Used-in reference information
 */
export interface UsedInReference {
  familyId: string;
  familyName: string;
  roleName: string;
  version: number;
}

/**
 * Service for managing nested family dependencies.
 * Provides API methods for Phase 3 nested families feature.
 */
@Injectable({
  providedIn: 'root',
})
export class NestedFamilyService {
  private readonly apiService = inject(ApiService);

  /**
   * Get nested family dependencies for a family
   * GET /api/families/{id}/dependencies
   */
  getDependencies(familyId: string): Observable<FamilyDependencies> {
    return this.apiService.get<FamilyDependencies>(`/families/${familyId}/dependencies`);
  }

  /**
   * Get pre-load summary before loading family to project
   * GET /api/families/{id}/pre-load-summary
   */
  getPreLoadSummary(familyId: string): Observable<PreLoadSummary> {
    return this.apiService.get<PreLoadSummary>(`/families/${familyId}/pre-load-summary`);
  }

  /**
   * Get families that use this family as a nested dependency
   * GET /api/families/{id}/used-in
   */
  getUsedIn(familyId: string): Observable<UsedInReference[]> {
    return this.apiService.get<UsedInReference[]>(`/families/${familyId}/used-in`);
  }
}
