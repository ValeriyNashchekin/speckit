import { ChangeDetectionStrategy, Component, inject, computed, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { ScannerService } from './services/scanner.service';
import { ScannerTableComponent } from './components/scanner-table.component';
import { ScannerFiltersComponent } from './components/scanner-filters.component';
import { UpdateProgressComponent } from './components/update-progress.component';
import { PreUpdatePreviewComponent } from './components/pre-update-preview.component';
import { ScannedFamily, FamilyScanStatus } from '../../core/models/scanner.models';

type FilterStatus = FamilyScanStatus | 'All';

/**
 * Main scanner component for detecting and updating families from library.
 */
@Component({
  selector: 'app-scanner',
  imports: [
    CommonModule,
    ButtonModule,
    ScannerTableComponent,
    ScannerFiltersComponent,
    UpdateProgressComponent,
    PreUpdatePreviewComponent,
  ],
  templateUrl: './scanner.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ScannerComponent implements OnInit {
  protected readonly scannerService = inject(ScannerService);

  // Current filter state
  protected currentFilter = signal<FilterStatus>('All');

  // Filtered families computed from service
  protected readonly filteredFamilies = computed(() =>
    this.scannerService.filterByStatus(this.currentFilter())
  );

  // Loading states
  protected readonly isScanning = this.scannerService.isScanning;
  protected readonly isUpdating = this.scannerService.isUpdating;
  protected readonly updateProgress = this.scannerService.updateProgress;
  protected readonly scanResult = this.scannerService.scanResult;
  protected readonly summary = this.scannerService.summary;

  // Preview states
  protected readonly previewData = this.scannerService.previewData;
  protected readonly isFetchingPreview = this.scannerService.isFetchingPreview;

  /**
   * Initialize component with auto-scan
   */
  ngOnInit(): void {
    // Auto-scan on load
    this.scan();
  }

  /**
   * Trigger project scan
   */
  protected scan(): void {
    this.currentFilter.set('All');
    this.scannerService.scanProject(false);
  }

  /**
   * Handle filter change from filters component
   */
  protected onFilterChange(filter: FilterStatus): void {
    this.currentFilter.set(filter);
  }

  /**
   * Handle update selected families - shows preview first
   */
  protected onUpdateSelected(families: ScannedFamily[]): void {
    const updates = families.map((f) => ({
      uniqueId: f.uniqueId,
      roleName: f.roleName,
    }));
    this.scannerService.requestPreview(updates);
  }

  /**
   * Handle preview confirmation - proceed with update
   */
  protected onPreviewConfirm(): void {
    this.scannerService.confirmPreviewAndUpdate();
  }

  /**
   * Handle preview cancellation
   */
  protected onPreviewCancel(): void {
    this.scannerService.cancelPreview();
  }

  /**
   * Handle stamp selected families
   */
  protected onStampSelected(families: ScannedFamily[]): void {
    const stamps = families
      .filter((f) => f.roleName)
      .map((f) => ({
        uniqueId: f.uniqueId,
        roleName: f.roleName!,
      }));

    if (stamps.length > 0) {
      this.scannerService.stampLegacy(stamps);
    }
  }
}
