import { ChangeDetectionStrategy, Component, computed, inject, input, output, signal } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { SelectModule } from 'primeng/select';
import { TooltipModule } from 'primeng/tooltip';
import { FormsModule } from '@angular/forms';
import { RevitBridgeService } from '../../../../core/services/revit-bridge.service';
import type { NestedLoadInfo, LoadPreviewEvent } from '../../../../core/models/webview-events.model';

/** Source type for nested family loading */
export type NestedLoadSource = 'rfa' | 'library' | 'project';

/**
 * User choice for nested family load source
 */
export interface NestedLoadChoice {
  familyName: string;
  source: NestedLoadSource;
  targetVersion?: number;
}

/** Option for dropdown selection */
interface SourceOption {
  label: string;
  value: NestedLoadSource;
  disabled?: boolean;
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
    SelectModule,
    TooltipModule,
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
  protected readonly selections = signal<Map<string, NestedLoadSource>>(new Map());

  // Base source options for dropdown
  protected readonly baseSourceOptions: SourceOption[] = [
    { label: 'Load from RFA', value: 'rfa' },
    { label: 'Update from Library', value: 'library' },
    { label: 'Keep Project Version', value: 'project' },
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

    return families.map(f => {
      const source = currentSelections.get(f.familyName) ?? 'rfa';
      return {
        familyName: f.familyName,
        source,
        targetVersion: source === 'library' ? f.libraryVersion : undefined,
      };
    });
  });

  // Computed: check if user has modified any selections from recommendations
  protected readonly hasUserModifications = computed(() => {
    const families = this.nestedFamilies();
    const currentSelections = this.selections();

    return families.some(f => {
      const current = currentSelections.get(f.familyName);
      const recommended = this.mapRecommendedToSource(f.recommendedAction);
      return current !== recommended;
    });
  });

  // Computed: count of families that will be loaded (not 'project' source)
  protected readonly loadCount = computed(() => {
    const currentSelections = this.selections();
    return this.nestedFamilies().filter(f =>
      currentSelections.get(f.familyName) !== 'project'
    ).length;
  });

  /**
   * Initialize selections with recommended actions when preview data changes
   */
  protected initializeSelections(): void {
    const families = this.nestedFamilies();
    const newSelections = new Map<string, NestedLoadSource>();

    families.forEach(f => {
      newSelections.set(f.familyName, this.mapRecommendedToSource(f.recommendedAction));
    });

    this.selections.set(newSelections);
  }

  /**
   * Map recommended action to source type
   */
  private mapRecommendedToSource(action: NestedLoadInfo['recommendedAction']): NestedLoadSource {
    switch (action) {
      case 'update_from_library':
        return 'library';
      case 'keep_project':
        return 'project';
      case 'load_from_rfa':
      case 'no_action':
      default:
        return 'rfa';
    }
  }

  /**
   * Get current selection for a family
   */
  protected getSelection(familyName: string): NestedLoadSource {
    return this.selections().get(familyName) ?? 'rfa';
  }

  /**
   * Get available source options for a nested family
   * Disables options that are not applicable (e.g., library if not in library)
   */
  protected getSourceOptions(nested: NestedLoadInfo): SourceOption[] {
    return this.baseSourceOptions.map(opt => ({
      ...opt,
      disabled: opt.value === 'library' && nested.libraryVersion === undefined ||
                opt.value === 'project' && nested.projectVersion === undefined,
    }));
  }

  /**
   * Update selection for a family
   */
  protected onSelectionChange(familyName: string, source: NestedLoadSource): void {
    this.selections.update(current => {
      const newMap = new Map(current);
      newMap.set(familyName, source);
      return newMap;
    });
  }

  /**
   * Check if current selection matches recommended action
   */
  protected isRecommendedSelection(nested: NestedLoadInfo): boolean {
    const current = this.getSelection(nested.familyName);
    const recommended = this.mapRecommendedToSource(nested.recommendedAction);
    return current === recommended;
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
   * Handle Load All button click - applies all recommended actions
   */
  protected onLoadAll(): void {
    // Reset all selections to recommended actions
    this.initializeSelections();
    this.performLoad();
  }

  /**
   * Handle Load Selected button click - applies user-modified choices
   */
  protected onLoadSelected(): void {
    this.performLoad();
  }

  /**
   * Perform the actual load operation
   */
  private performLoad(): void {
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
   * Handle Load button click (legacy - uses current selections)
   * @deprecated Use onLoadAll or onLoadSelected instead
   */
  protected onLoad(): void {
    this.performLoad();
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
