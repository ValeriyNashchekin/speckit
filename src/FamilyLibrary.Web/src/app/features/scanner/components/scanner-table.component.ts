import { ChangeDetectionStrategy, Component, input, output, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { ScannedFamily, FamilyScanStatus } from '../../../core/models/scanner.models';

/**
 * Table component for displaying scanned families with selection and actions.
 */
@Component({
  selector: 'app-scanner-table',
  imports: [CommonModule, TableModule, ButtonModule, TagModule, TooltipModule],
  templateUrl: './scanner-table.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ScannerTableComponent {
  // Inputs
  families = input<ScannedFamily[]>([]);
  loading = input<boolean>(false);

  // Outputs
  updateSelected = output<ScannedFamily[]>();
  stampSelected = output<ScannedFamily[]>();
  familyAction = output<{ family: ScannedFamily; action: 'update' | 'stamp' }>();

  // Selection state
  protected readonly selectedFamilies = signal<ScannedFamily[]>([]);

  // Computed values for button states
  protected readonly hasUpdatableSelected = computed(() =>
    this.selectedFamilies().some(
      (f) => f.status === 'UpdateAvailable' || f.status === 'LegacyMatch'
    )
  );

  protected readonly hasStampableSelected = computed(() =>
    this.selectedFamilies().some(
      (f) => f.status === 'LegacyMatch' || f.status === 'Unmatched'
    )
  );

  protected readonly selectedCount = computed(
    () => this.selectedFamilies().length
  );

  // Table configuration
  protected readonly rows = 50;

  /**
   * Handle selection toggle
   */
  protected onSelectionChange(value: ScannedFamily[]): void {
    this.selectedFamilies.set(value);
  }

  /**
   * Emit update event for selected families
   */
  protected onUpdateSelected(): void {
    const updatable = this.selectedFamilies().filter(
      (f) => f.status === 'UpdateAvailable' || f.status === 'LegacyMatch'
    );
    if (updatable.length > 0) {
      this.updateSelected.emit(updatable);
    }
  }

  /**
   * Emit stamp event for selected families
   */
  protected onStampSelected(): void {
    const stampable = this.selectedFamilies().filter(
      (f) => f.status === 'LegacyMatch' || f.status === 'Unmatched'
    );
    if (stampable.length > 0) {
      this.stampSelected.emit(stampable);
    }
  }

  /**
   * Update single family
   */
  protected onUpdateFamily(family: ScannedFamily): void {
    this.updateSelected.emit([family]);
  }

  /**
   * Stamp single family
   */
  protected onStampFamily(family: ScannedFamily): void {
    this.stampSelected.emit([family]);
  }

  /**
   * Get display label for status
   */
  protected getStatusLabel(status: FamilyScanStatus): string {
    const labels: Record<FamilyScanStatus, string> = {
      UpToDate: 'Up to date',
      UpdateAvailable: 'Update Available',
      LegacyMatch: 'Legacy Match',
      Unmatched: 'Unmatched',
      LocalModified: 'Local Modified',
    };
    return labels[status];
  }

  /**
   * Get tag severity for status
   */
  protected getStatusSeverity(
    status: FamilyScanStatus
  ): 'success' | 'warn' | 'info' | 'secondary' | 'contrast' {
    const severities: Record<FamilyScanStatus, 'success' | 'warn' | 'info' | 'secondary' | 'contrast'> = {
      UpToDate: 'success',
      UpdateAvailable: 'warn',
      LegacyMatch: 'info',
      Unmatched: 'secondary',
      LocalModified: 'contrast',
    };
    return severities[status];
  }

  /**
   * Track by unique ID
   */
  protected trackByUniqueId(_: number, family: ScannedFamily): string {
    return family.uniqueId;
  }

  /**
   * Clear selection
   */
  protected clearSelection(): void {
    this.selectedFamilies.set([]);
  }
}
