import { Injectable, signal, computed, inject } from '@angular/core';
import { RevitBridgeService } from '../../../core/services/revit-bridge.service';
import {
  ScanResult,
  ScannedFamily,
  FamilyScanStatus,
  ChangeSet,
} from '../../../core/models/scanner.models';
import {
  Phase2PluginEventTypes,
  Phase2UiEventTypes,
} from '../../../core/models/webview-events.model';

/**
 * Progress payload from Revit during family updates
 */
interface UpdateProgressPayload {
  completed: number;
  total: number;
  currentFamily: string;
  success: number;
  failed: number;
}

/**
 * Preview data for a single family update
 */
export interface FamilyPreviewData {
  uniqueId: string;
  familyName: string;
  roleName?: string;
  changes: ChangeSet;
}

/**
 * Service for managing scanner operations and state.
 * Handles communication with Revit plugin for scanning and updating families.
 */
@Injectable({ providedIn: 'root' })
export class ScannerService {
  private readonly revitBridge = inject(RevitBridgeService);

  // State signals
  private readonly _scanResult = signal<ScanResult | null>(null);
  private readonly _isScanning = signal(false);
  private readonly _updateProgress = signal<UpdateProgressPayload | null>(null);
  private readonly _isUpdating = signal(false);
  private readonly _previewData = signal<FamilyPreviewData | null>(null);
  private readonly _isFetchingPreview = signal(false);

  // Pending updates queue for batch processing with preview
  private pendingUpdates: Array<{ uniqueId: string; roleName?: string }> = [];

  // Public readonly signals
  readonly scanResult = this._scanResult.asReadonly();
  readonly isScanning = this._isScanning.asReadonly();
  readonly updateProgress = this._updateProgress.asReadonly();
  readonly isUpdating = this._isUpdating.asReadonly();
  readonly previewData = this._previewData.asReadonly();
  readonly isFetchingPreview = this._isFetchingPreview.asReadonly();

  // Computed values
  readonly summary = computed(() => this._scanResult()?.summary);
  readonly families = computed(() => this._scanResult()?.families ?? []);

  constructor() {
    this.setupEventListeners();
  }

  /**
   * Set up event listeners for Revit plugin messages
   */
  private setupEventListeners(): void {
    // Handle scan result
    this.revitBridge
      .on<ScanResult>(Phase2PluginEventTypes.REVIT_SCAN_RESULT)
      .subscribe((result) => {
        this._scanResult.set(result);
        this._isScanning.set(false);
      });

    // Handle update progress
    this.revitBridge
      .on<UpdateProgressPayload>(Phase2PluginEventTypes.REVIT_UPDATE_PROGRESS)
      .subscribe((progress) => {
        this._updateProgress.set(progress);
      });

    // Handle update complete
    this.revitBridge
      .on<{ total: number; success: number; failed: number }>(
        Phase2PluginEventTypes.REVIT_UPDATE_COMPLETE
      )
      .subscribe(() => {
        this._isUpdating.set(false);
        this._updateProgress.set(null);
        this.pendingUpdates = [];
      });

    // Handle changes result for preview
    this.revitBridge
      .on<{ familyUniqueId: string; changes: ChangeSet }>(
        Phase2PluginEventTypes.REVIT_CHANGES_RESULT
      )
      .subscribe((result) => {
        this._isFetchingPreview.set(false);
        const pendingItem = this.pendingUpdates[0];
        if (pendingItem) {
          const family = this.findFamilyById(pendingItem.uniqueId);
          this._previewData.set({
            uniqueId: pendingItem.uniqueId,
            familyName: family?.familyName ?? 'Unknown',
            roleName: pendingItem.roleName,
            changes: result.changes,
          });
        }
      });
  }

  /**
   * Find family by uniqueId in scan results
   */
  private findFamilyById(uniqueId: string): ScannedFamily | undefined {
    return this._scanResult()?.families.find((f) => f.uniqueId === uniqueId);
  }

  /**
   * Request project scan from Revit
   */
  scanProject(includeSystemFamilies = false): void {
    this._isScanning.set(true);
    this._scanResult.set(null);
    this.revitBridge.send(Phase2UiEventTypes.UI_SCAN_PROJECT, {
      includeSystemFamilies,
    });
  }

  /**
   * Request preview for families before update.
   * Processes one family at a time.
   */
  requestPreview(
    families: Array<{ uniqueId: string; roleName?: string }>
  ): void {
    if (families.length === 0) return;

    // Store pending updates and clear previous preview
    this.pendingUpdates = [...families];
    this._previewData.set(null);

    // Request changes for first family
    const first = families[0];
    this._isFetchingPreview.set(true);
    this.revitBridge.send(Phase2UiEventTypes.UI_GET_CHANGES, {
      uniqueId: first.uniqueId,
    });
  }

  /**
   * Confirm current preview and proceed with update.
   * After update, if more families pending, request next preview.
   */
  confirmPreviewAndUpdate(): void {
    const preview = this._previewData();
    if (!preview) return;

    // Clear preview and start update for current family
    this._previewData.set(null);
    this._isUpdating.set(true);

    // Remove processed item from queue
    this.pendingUpdates.shift();

    // Send update for confirmed family
    this.revitBridge.send(Phase2UiEventTypes.UI_UPDATE_FAMILIES, {
      families: [{ uniqueId: preview.uniqueId, roleName: preview.roleName }],
      showPreview: false,
    });
  }

  /**
   * Cancel preview and clear pending updates
   */
  cancelPreview(): void {
    this._previewData.set(null);
    this.pendingUpdates = [];
  }

  /**
   * Skip current family and request preview for next
   */
  skipCurrentAndShowNext(): void {
    this._previewData.set(null);
    this.pendingUpdates.shift();

    if (this.pendingUpdates.length > 0) {
      const next = this.pendingUpdates[0];
      this._isFetchingPreview.set(true);
      this.revitBridge.send(Phase2UiEventTypes.UI_GET_CHANGES, {
        uniqueId: next.uniqueId,
      });
    }
  }

  /**
   * Check if there are more pending updates after current
   */
  hasPendingUpdates(): boolean {
    return this.pendingUpdates.length > 1;
  }

  /**
   * Get count of pending updates
   */
  getPendingCount(): number {
    return this.pendingUpdates.length;
  }

  /**
   * Request family updates from Revit (without preview)
   */
  updateFamilies(
    families: Array<{ uniqueId: string; roleName?: string }>,
    showPreview = true
  ): void {
    if (showPreview) {
      this.requestPreview(families);
    } else {
      this._isUpdating.set(true);
      this.revitBridge.send(Phase2UiEventTypes.UI_UPDATE_FAMILIES, {
        families,
        showPreview: false,
      });
    }
  }

  /**
   * Stamp families with legacy roles
   */
  stampLegacy(
    families: Array<{ uniqueId: string; roleName: string }>
  ): void {
    this.revitBridge.send(Phase2UiEventTypes.UI_STAMP_LEGACY, {
      families,
    });
  }

  /**
   * Filter families by status
   */
  filterByStatus(status: FamilyScanStatus | 'All'): ScannedFamily[] {
    const all = this._scanResult()?.families ?? [];
    if (status === 'All') return all;
    return all.filter((f) => f.status === status);
  }

  /**
   * Clear scan result
   */
  clearScanResult(): void {
    this._scanResult.set(null);
  }
}
