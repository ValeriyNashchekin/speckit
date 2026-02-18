import { ChangeDetectionStrategy, Component, computed, inject, input, output, signal } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { SelectButtonModule } from 'primeng/selectbutton';
import { FormsModule } from '@angular/forms';
import { RevitBridgeService } from '../../../../core/services/revit-bridge.service';
import type { NestedLoadInfo, LoadPreviewEvent } from '../../../../core/models/webview-events.model';

/**
 * User choice for nested family load source
 */
export interface NestedLoadChoice {
  familyName: string;
  source: 'rfa' | 'library';
  targetVersion?: number;
}

/**
 * Internal selection state for each nested family
 */
interface NestedSelection {
  info: NestedLoadInfo;
  selectedSource: 'rfa' | 'library';
}

/**
 * Pre-load summary dialog component.
 * Shows nested families with version comparison and allows user to choose load source.
 */
@Component({
  selector: 'app-pre-load-summary',
  imports: [
    DialogModule,
    ButtonModule,
    TableModule,
    TagModule,
    SelectButtonModule,
    FormsModule,
  ],
  templateUrl: './pre-load-summary.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PreLoadSummaryComponent {
  private readonly revitBridge = inject(RevitBridgeService);

  // Inputs
  visible = input<boolean>(false);
  previewData = input<LoadPreviewEvent['payload'] | null>(null);

  // Outputs
  visibleChange = output<boolean>();
  loadConfirmed = output<NestedLoadChoice[]>();
  cancelled = output<void>();

  // Internal state for user selections per nested family
  protected readonly selections = signal<Map<string, 'rfa' | 'library'>>(new Map());

  // Source options for select button
  protected readonly sourceOptions = [
    { label: 'From RFA', value: 'rfa' as const },
    { label: 'From Library', value: 'library' as const },
  ];

  // Computed: nested families list
  protected readonly nestedFamilies = computed(() => this.previewData()?.nestedFamilies ?? []);

  // Computed: parent family info
  protected readonly parentFamily = computed(() => this.previewData()?.parentFamily);

  // Computed: summary stats
  protected readonly summary = computed(() => this.previewData()?.summary);

  // Computed: dialog header
  protected readonly dialogHeader = computed(() => {
    const parent = this.parentFamily();
    if (parent) {
      return `Load ${parent.familyName} v${parent.version}`;
    }
    return 'Load Family';
  });

  // Computed: count of conflicts
  protected readonly conflictCount = computed(() => this.summary()?.conflictCount ?? 0);

  // Computed: check if any conflicts exist
  protected readonly hasConflicts = computed(() => this.conflictCount() > 0);

  // Computed: all selections made
  protected readonly allSelectionsMade = computed(() => {
    const families = this.nestedFamilies();
    const currentSelections = this.selections();
    return families.every(f => currentSelections.has(f.familyName));
  });

  // Computed: build final choices array
  protected readonly choices = computed<NestedLoadChoice[]>(() => {
    const families = this.nestedFamilies();
    const currentSelections = this.selections();
    
    return families.map(f => ({
      familyName: f.familyName,
      source: currentSelections.get(f.familyName) ?? 'rfa',
      targetVersion: currentSelections.get(f.familyName) === 'library' 
        ? f.libraryVersion 
        : undefined,
    }));
  });

  /**
   * Initialize selections with recommended actions when preview data changes
   */
  protected initializeSelections(): void {
    const families = this.nestedFamilies();
    const newSelections = new Map<string, 'rfa' | 'library'>();
    
    families.forEach(f => {
      // Use recommended action as default
      if (f.recommendedAction === 'update_from_library') {
        newSelections.set(f.familyName, 'library');
      } else {
        newSelections.set(f.familyName, 'rfa');
      }
    });
    
    this.selections.set(newSelections);
  }

  /**
   * Get current selection for a family
   */
  protected getSelection(familyName: string): 'rfa' | 'library' {
    return this.selections().get(familyName) ?? 'rfa';
  }

  /**
   * Update selection for a family
   */
  protected onSelectionChange(familyName: string, source: 'rfa' | 'library'): void {
    this.selections.update(current => {
      const newMap = new Map(current);
      newMap.set(familyName, source);
      return newMap;
    });
  }

  /**
   * Get severity for version tag
   */
  protected getVersionSeverity(hasConflict: boolean): 'danger' | 'success' {
    return hasConflict ? 'danger' : 'success';
  }

  /**
   * Get severity for recommended action badge
   */
  protected getActionSeverity(action: NestedLoadInfo['recommendedAction']): 'success' | 'warn' | 'info' | 'secondary' {
    switch (action) {
      case 'load_from_rfa':
        return 'info';
      case 'update_from_library':
        return 'success';
      case 'keep_project':
        return 'warn';
      case 'no_action':
      default:
        return 'secondary';
    }
  }

  /**
   * Get label for recommended action
   */
  protected getActionLabel(action: NestedLoadInfo['recommendedAction']): string {
    switch (action) {
      case 'load_from_rfa':
        return 'Load from RFA';
      case 'update_from_library':
        return 'Update from Library';
      case 'keep_project':
        return 'Keep Project';
      case 'no_action':
      default:
        return 'No Action';
    }
  }

  /**
   * Format version display (handles undefined)
   */
  protected formatVersion(version: number | undefined): string {
    return version !== undefined ? `v${version}` : '-';
  }

  /**
   * Handle Load button click
   */
  protected onLoad(): void {
    const parent = this.parentFamily();
    if (!parent) {
      return;
    }

    // Send event to Revit plugin
    this.revitBridge.loadWithNested({
      parentFamilyId: parent.familyId,
      nestedChoices: this.choices(),
    });

    // Emit event and close dialog
    this.loadConfirmed.emit(this.choices());
    this.visibleChange.emit(false);
  }

  /**
   * Handle Cancel button click
   */
  protected onCancel(): void {
    this.cancelled.emit();
    this.visibleChange.emit(false);
  }

  /**
   * Handle dialog hide
   */
  protected onHide(): void {
    this.visibleChange.emit(false);
  }

  /**
   * Track by family name for table
   */
  protected trackByFamilyName(_index: number, item: NestedLoadInfo): string {
    return item.familyName;
  }
}
