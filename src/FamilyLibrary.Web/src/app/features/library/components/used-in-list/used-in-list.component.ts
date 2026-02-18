import { ChangeDetectionStrategy, Component, computed, inject, input, signal } from '@angular/core';
import { Router } from '@angular/router';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { BadgeModule } from 'primeng/badge';
import { SkeletonModule } from 'primeng/skeleton';
import { NestedFamilyService, UsedInReference } from '../../../../core/services/nested-family.service';

/**
 * Extended reference with version comparison data
 */
interface UsedInReferenceWithStatus extends UsedInReference {
  isOutdated: boolean;
  currentVersion?: number;
}

@Component({
  selector: 'app-used-in-list',
  imports: [TableModule, TagModule, TooltipModule, BadgeModule, SkeletonModule],
  templateUrl: './used-in-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UsedInListComponent {
  private readonly nestedFamilyService = inject(NestedFamilyService);
  private readonly router = inject(Router);

  /**
   * Family ID to get used-in references for
   */
  familyId = input.required<string>();

  /**
   * Current version of the family (for comparison)
   */
  currentVersion = input<number | undefined>(undefined);

  /**
   * Whether to show the component header
   */
  showHeader = input<boolean>(true);

  // State
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly references = signal<UsedInReferenceWithStatus[]>([]);

  // Computed
  protected readonly outdatedCount = computed(() =>
    this.references().filter(r => r.isOutdated).length
  );

  protected readonly totalCount = computed(() =>
    this.references().length
  );

  constructor() {
    // Load data when familyId is available
    this.loadUsedIn();
  }

  /**
   * Load used-in references from API
   */
  protected loadUsedIn(): void {
    const id = this.familyId();
    if (!id) {
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    this.nestedFamilyService.getUsedIn(id).subscribe({
      next: data => {
        const currentVer = this.currentVersion();
        const referencesWithStatus: UsedInReferenceWithStatus[] = data.map(ref => ({
          ...ref,
          isOutdated: currentVer !== undefined && ref.version < currentVer,
          currentVersion: currentVer,
        }));
        this.references.set(referencesWithStatus);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load used-in references');
        this.loading.set(false);
      },
    });
  }

  /**
   * Navigate to family detail page
   */
  protected navigateToFamily(familyId: string): void {
    this.router.navigate(['/library', familyId]);
  }

  /**
   * Get severity for version tag
   */
  protected getVersionSeverity(ref: UsedInReferenceWithStatus): 'danger' | 'success' {
    return ref.isOutdated ? 'danger' : 'success';
  }

  /**
   * Track by family ID for ngFor
   */
  protected trackByFamilyId(_index: number, item: UsedInReferenceWithStatus): string {
    return item.familyId;
  }
}
