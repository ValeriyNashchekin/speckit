import { Injectable, signal, computed, inject } from '@angular/core';
import { RevitBridgeService } from '../../../core/services/revit-bridge.service';
import {
  ScanResult,
  ScannedFamily,
  FamilyScanStatus,
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

  // Public readonly signals
  readonly scanResult = this._scanResult.asReadonly();
  readonly isScanning = this._isScanning.asReadonly();
  readonly updateProgress = this._updateProgress.asReadonly();
  readonly isUpdating = this._isUpdating.asReadonly();

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
      });
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
   * Request family updates from Revit
   */
  updateFamilies(
    families: Array<{ uniqueId: string; roleName?: string }>,
    showPreview = true
  ): void {
    this._isUpdating.set(true);
    this.revitBridge.send(Phase2UiEventTypes.UI_UPDATE_FAMILIES, {
      families,
      showPreview,
    });
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
