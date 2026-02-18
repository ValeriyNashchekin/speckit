import { ChangeDetectionStrategy, Component, computed, inject, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { ScannedFamily, FamilyScanStatus } from '../../../core/models/scanner.models';
import { ViewChangesModalComponent, ChangeSet } from '../../library/components/view-changes-modal/view-changes-modal.component';
import { ApiService } from '../../../core/api/api.service';

/**
 * Table component for displaying scanned families with selection and actions.
 */
@Component({
  selector: 'app-scanner-table',
  imports: [CommonModule, TableModule, ButtonModule, TagModule, TooltipModule, ViewChangesModalComponent],
  templateUrl: './scanner-table.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ScannerTableComponent {
  private readonly apiService = inject(ApiService);

  families = input<ScannedFamily[]>([]);
  loading = input<boolean>(false);

  updateSelected = output<ScannedFamily[]>();
  updateAllAvailable = output<void>();
  stampSelected = output<ScannedFamily[]>();
  familyAction = output<{ family: ScannedFamily; action: 'update' | 'stamp' }>();

  // Selection state
  protected readonly selectedFamilies = signal<ScannedFamily[]>([]);

  // View changes modal state
  protected readonly changesModalVisible = signal(false);
  protected readonly selectedChanges = signal<ChangeSet | null>(null);
  protected readonly selectedFamilyName = signal('');

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

  protected readonly hasAnyUpdatable = computed(() =>
    this.families().some(
      (f) => f.status === 'UpdateAvailable'
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

  protected onUpdateAllAvailable(): void {
    this.updateAllAvailable.emit();
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

  protected onViewChanges(family: ScannedFamily): void {
    this.selectedFamilyName.set(family.familyName);
    this.changesModalVisible.set(true);
    this.selectedChanges.set(null);

    if (family.localHash) {
      this.apiService
        .post<{ items: Array<{ category: string; previousValue?: string; currentValue?: string; addedItems?: string[]; removedItems?: string[]; parameterChanges?: Array<{ name: string; previousValue?: string; currentValue?: string }> }>; hasChanges: boolean }>(
          `/families/${family.uniqueId}/local-changes`,
          { localSnapshotJson: family.localHash },
        )
        .subscribe({
          next: result => {
            this.selectedChanges.set({ changes: result.items });
          },
          error: () => {
            this.selectedChanges.set({ changes: [] });
          },
        });
    } else {
      this.selectedChanges.set({ changes: [] });
    }
  }

  /**
   * Handle modal visibility change
   */
  protected onChangesModalVisibleChange(visible: boolean): void {
    this.changesModalVisible.set(visible);
    if (!visible) {
      this.selectedChanges.set(null);
      this.selectedFamilyName.set('');
    }
  }

  /**
   * Handle publish from modal
   */
  protected onPublishChanges(): void {
    // TODO: Implement publish logic
    this.onChangesModalVisibleChange(false);
  }

  /**
   * Handle cancel from modal
   */
  protected onCancelChanges(): void {
    this.onChangesModalVisibleChange(false);
  }
}
