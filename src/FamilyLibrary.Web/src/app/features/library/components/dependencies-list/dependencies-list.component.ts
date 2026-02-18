import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { ButtonModule } from 'primeng/button';
import { NestedFamilyInfo } from '../../../../core/services/nested-family.service';

@Component({
  selector: 'app-dependencies-list',
  imports: [ButtonModule, TableModule, TagModule, TooltipModule],
  templateUrl: './dependencies-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DependenciesListComponent {
  /**
   * Nested family dependencies to display
   */
  dependencies = input<NestedFamilyInfo[]>([]);

  /**
   * Parent family name for context
   */
  parentFamilyName = input<string>('');

  /**
   * Whether data is loading
   */
  loading = input<boolean>(false);

  /**
   * Computed count of shared families
   */
  protected readonly sharedCount = computed(() =>
    this.dependencies().filter(d => d.isShared).length
  );

  /**
   * Computed count of families not in library
   */
  protected readonly notPublishedCount = computed(() =>
    this.dependencies().filter(d => d.status === 'not_published').length
  );

  /**
   * Get severity for status tag
   */
  protected getStatusSeverity(status: NestedFamilyInfo['status']): 'success' | 'warn' | 'secondary' {
    switch (status) {
      case 'ready':
        return 'success';
      case 'not_published':
        return 'warn';
      case 'no_role':
      default:
        return 'secondary';
    }
  }

  /**
   * Get label for status
   */
  protected getStatusLabel(status: NestedFamilyInfo['status']): string {
    switch (status) {
      case 'ready':
        return 'Ready';
      case 'not_published':
        return 'Not Published';
      case 'no_role':
        return 'No Role';
      default:
        return status;
    }
  }

  /**
   * Track by family name for ngFor
   */
  protected trackByFamilyName(_index: number, item: NestedFamilyInfo): string {
    return item.familyName;
  }
}
