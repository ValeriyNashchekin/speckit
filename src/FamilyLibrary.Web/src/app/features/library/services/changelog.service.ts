import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../../core/api/api.service';
import { FamilyVersion } from '../../../core/models/family.model';
import { ChangeSet } from '../../../core/models/scanner.models';

@Injectable({
  providedIn: 'root',
})
export class ChangelogService {
  private readonly apiService = inject(ApiService);

  /**
   * Get changes between two versions of a family.
   */
  getChanges(
    familyId: string,
    fromVersion: number,
    toVersion: number,
  ): Observable<ChangeSet> {
    return this.apiService.get<ChangeSet>(
      `/families/${familyId}/changes`,
      {
        params: {
          fromVersion,
          toVersion,
        },
      },
    );
  }

  /**
   * Get all versions for a family (used for version selection in changelog).
   */
  getVersions(familyId: string): Observable<FamilyVersion[]> {
    return this.apiService.get<FamilyVersion[]>(`/families/${familyId}/versions`);
  }
}
