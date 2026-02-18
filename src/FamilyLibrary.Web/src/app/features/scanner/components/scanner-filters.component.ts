import { ChangeDetectionStrategy, Component, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { ScanSummary, FamilyScanStatus } from '../../../core/models/scanner.models';

type FilterStatus = FamilyScanStatus | 'All';

interface FilterOption {
  status: FilterStatus;
  label: string;
  count: number;
  severity: 'success' | 'warn' | 'info' | 'secondary' | 'primary';
}

/**
 * Filter component for scanner results with summary badges.
 */
@Component({
  selector: 'app-scanner-filters',
  imports: [CommonModule, ButtonModule],
  templateUrl: './scanner-filters.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ScannerFiltersComponent {
  // Inputs
  summary = input.required<ScanSummary>();

  // Outputs
  filterChange = output<FilterStatus>();

  // Current filter state
  protected readonly currentFilter = signal<FilterStatus>('All');

  /**
   * Get filter options with counts from summary
   */
  protected get filterOptions(): FilterOption[] {
    const summary = this.summary();
    return [
      {
        status: 'All',
        label: 'All',
        count: this.totalCount(summary),
        severity: 'primary',
      },
      {
        status: 'UpToDate',
        label: 'Up to date',
        count: summary.upToDate,
        severity: 'success',
      },
      {
        status: 'UpdateAvailable',
        label: 'Update Available',
        count: summary.updateAvailable,
        severity: 'warn',
      },
      {
        status: 'LegacyMatch',
        label: 'Legacy Match',
        count: summary.legacyMatch,
        severity: 'info',
      },
      {
        status: 'Unmatched',
        label: 'Unmatched',
        count: summary.unmatched,
        severity: 'secondary',
      },
      {
        status: 'LocalModified',
        label: 'Local Modified',
        count: summary.localModified,
        severity: 'secondary',
      },
    ];
  }

  /**
   * Calculate total count
   */
  private totalCount(summary: ScanSummary): number {
    return (
      summary.upToDate +
      summary.updateAvailable +
      summary.legacyMatch +
      summary.unmatched +
      summary.localModified
    );
  }

  /**
   * Set filter and emit change
   */
  protected setFilter(status: FilterStatus): void {
    this.currentFilter.set(status);
    this.filterChange.emit(status);
  }

  /**
   * Get button classes based on active state
   */
  protected getButtonClasses(option: FilterOption): string {
    const isActive = this.currentFilter() === option.status;
    const baseClasses = 'px-3 py-2 text-sm font-medium rounded-md transition-colors';
    
    if (isActive) {
      return `${baseClasses} bg-primary-500 text-white`;
    }
    
    return `${baseClasses} bg-gray-100 text-gray-700 hover:bg-gray-200`;
  }
}
