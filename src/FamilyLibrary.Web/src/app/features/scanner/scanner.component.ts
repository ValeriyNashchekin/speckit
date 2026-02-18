import {
  ChangeDetectionStrategy,
  Component,
  inject,
  computed,
  signal,
  OnInit,
  OnDestroy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { Subject, takeUntil } from 'rxjs';
import { ScannerService, OperationNotification } from './services/scanner.service';
import { ScannerTableComponent } from './components/scanner-table.component';
import { ScannerFiltersComponent } from './components/scanner-filters.component';
import { UpdateProgressComponent } from './components/update-progress.component';
import { PreUpdatePreviewComponent } from './components/pre-update-preview.component';
import { MaterialFallbackDialogComponent, MaterialFallbackResult } from './components/material-fallback-dialog.component';
import { ScannedFamily, FamilyScanStatus } from '../../core/models/scanner.models';
import { RevitBridgeService } from '../../core/services/revit-bridge.service';
import { MaterialFallbackEvent } from '../../core/models/webview-events.model';

type FilterStatus = FamilyScanStatus | 'All';

// Toast message durations
const SUCCESS_LIFE = 3000;
const ERROR_LIFE = 5000;

/**
 * Main scanner component for detecting and updating families from library.
 */
@Component({
  selector: 'app-scanner',
  imports: [
    CommonModule,
    ButtonModule,
    ToastModule,
    ScannerTableComponent,
    ScannerFiltersComponent,
    UpdateProgressComponent,
    PreUpdatePreviewComponent,
    MaterialFallbackDialogComponent,
  ],
  templateUrl: './scanner.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ScannerComponent implements OnInit, OnDestroy {
  protected readonly scannerService = inject(ScannerService);
  private readonly messageService = inject(MessageService);
  private readonly revitBridge = inject(RevitBridgeService);
  private readonly destroy$ = new Subject<void>();

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

  // Material fallback dialog state
  protected readonly materialFallbackVisible = signal(false);
  protected readonly materialFallbackEvent = signal<MaterialFallbackEvent['payload'] | null>(null);
  protected readonly currentProjectId = signal<string>('');

  /**
   * Initialize component with auto-scan and notification subscription
   */
  ngOnInit(): void {
    // Subscribe to operation notifications
    this.scannerService.notification$
      .pipe(takeUntil(this.destroy$))
      .subscribe((notification) => this.showNotification(notification));

    // Subscribe to material fallback events from Revit
    this.revitBridge.onMaterialFallback()
      .pipe(takeUntil(this.destroy$))
      .subscribe((payload) => this.handleMaterialFallback(payload));

    // Auto-scan on load
    this.scan();
  }

  /**
   * Clean up subscriptions
   */
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Show toast notification based on operation result
   */
  private showNotification(notification: OperationNotification): void {
    const life = notification.type === 'success' ? SUCCESS_LIFE : ERROR_LIFE;

    this.messageService.add({
      severity: notification.type === 'success' ? 'success' : 'error',
      summary:
        notification.type === 'success'
          ? 'Success'
          : this.getErrorTitle(notification.operation),
      detail: notification.details
        ? `${notification.message}: ${notification.details}`
        : notification.message,
      life,
    });
  }

  /**
   * Get error title based on operation type
   */
  private getErrorTitle(
    operation: 'scan' | 'update' | 'preview'
  ): string {
    switch (operation) {
      case 'scan':
        return 'Scan Failed';
      case 'update':
        return 'Update Failed';
      case 'preview':
        return 'Preview Failed';
    }
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

  /**
   * Handle material fallback event from Revit plugin
   */
  private handleMaterialFallback(payload: MaterialFallbackEvent['payload']): void {
    this.materialFallbackEvent.set(payload);
    this.materialFallbackVisible.set(true);
  }

  /**
   * Handle material fallback resolved by user
   */
  protected onMaterialFallbackResolved(result: MaterialFallbackResult): void {
    console.log('[Scanner] Material fallback resolved:', result);
    this.materialFallbackVisible.set(false);
    this.materialFallbackEvent.set(null);

    // Show notification about the resolution
    this.messageService.add({
      severity: 'success',
      summary: 'Material Selected',
      detail: `Using "${result.projectMaterialName}" for the update`,
      life: 3000,
    });

    // The mapping is already sent to plugin if rememberMapping was checked
  }

  /**
   * Handle material fallback dialog closed without selection
   */
  protected onMaterialFallbackClosed(): void {
    this.materialFallbackVisible.set(false);
    this.materialFallbackEvent.set(null);

    this.messageService.add({
      severity: 'warn',
      summary: 'Update Cancelled',
      detail: 'Material selection was cancelled',
      life: 3000,
    });
  }
}
