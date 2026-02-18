import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { SkeletonModule } from 'primeng/skeleton';
import { TooltipModule } from 'primeng/tooltip';
import { CheckboxModule } from 'primeng/checkbox';
import { TabsModule } from 'primeng/tabs';
import { MessageService } from 'primeng/api';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { LibraryService } from '../../services/library.service';
import { FamilyDetail, FamilyVersion, TypeCatalogEntry } from '../../../../core/models/family.model';
import { ChangelogComponent } from '../changelog/changelog.component';
import { PreLoadSummaryComponent } from '../../../scanner/components/pre-load-summary/pre-load-summary.component';
import { UsedInListComponent } from '../used-in-list/used-in-list.component';
import { RevitBridgeService } from '../../../../core/services/revit-bridge.service';
import type { LoadPreviewEvent } from '../../../../core/models/webview-events.model';

declare global {
  interface Window {
    revitBridge?: {
      postMessage: (message: RevitBridgeMessage) => void;
    };
  }
}

interface RevitBridgeMessage {
  type: string;
  payload: unknown;
}

interface RevitFamilyLoadedEvent {
  success: boolean;
  familyName?: string;
  error?: string;
}

interface RevitFamilyLoadedDetail {
  detail: RevitFamilyLoadedEvent;
}

@Component({
  selector: 'app-family-detail',
  imports: [
    CardModule,
    TableModule,
    TagModule,
    ButtonModule,
    SkeletonModule,
    TooltipModule,
    CheckboxModule,
    TabsModule,
    FormsModule,
    DatePipe,
    ChangelogComponent,
    PreLoadSummaryComponent,
    UsedInListComponent,
  ],
  templateUrl: './family-detail.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FamilyDetailComponent {
  private readonly libraryService = inject(LibraryService);
  private readonly route = inject(ActivatedRoute);
  private readonly messageService = inject(MessageService);
  private readonly revitBridge = inject(RevitBridgeService);

  protected readonly family = signal<FamilyDetail | null>(null);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  // Tab state
  protected readonly activeTab = signal<string>('overview');

  // Type Catalog state
  protected readonly selectedTypes = signal<Set<number>>(new Set());

  // Pre-load summary dialog state
  protected readonly preLoadDialogVisible = signal(false);
  protected readonly preLoadData = signal<LoadPreviewEvent['payload'] | null>(null);

  // Computed properties
  protected readonly familyId = computed(() => this.family()?.id ?? '');

  // Type Catalog computed properties
  protected readonly hasTypeCatalog = computed(() => {
    const familyData = this.family();
    return familyData?.typeCatalog && familyData.typeCatalog.types.length > 0;
  });

  protected readonly typeCatalogFields = computed(() => {
    return this.family()?.typeCatalog?.fields ?? [];
  });

  protected readonly typeCatalogTypes = computed(() => {
    return this.family()?.typeCatalog?.types ?? [];
  });

  protected readonly selectedTypesCount = computed(() => this.selectedTypes().size);

  constructor() {
    this.loadFamily();
    this.subscribeToLoadPreview();
  }

  /**
   * Subscribe to revit:load:preview events from Revit plugin
   */
  private subscribeToLoadPreview(): void {
    this.revitBridge.onLoadPreview()
      .pipe(takeUntilDestroyed())
      .subscribe({
        next: (payload) => {
          this.preLoadData.set(payload);
          this.preLoadDialogVisible.set(true);
        },
      });
  }

  protected loadFamily(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.error.set('Family ID not provided');
      this.loading.set(false);
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    this.libraryService.getFamilyById(id).subscribe({
      next: data => {
        this.family.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load family details');
        this.loading.set(false);
      },
    });
  }

  protected downloadVersion(version: FamilyVersion): void {
    // TODO: Implement download logic via RevitBridgeService
    console.log('Download version:', version);
  }

  protected loadFamilyToProject(): void {
    const familyData = this.family();
    if (!familyData) {
      return;
    }

    const revitBridge = window.revitBridge;
    if (revitBridge) {
      revitBridge.postMessage({
        type: 'ui:load-family',
        payload: {
          familyId: familyData.id,
          version: familyData.currentVersion,
        },
      });

      this.subscribeToLoadResult();
    } else {
      const currentVersion = familyData.versions.find(v => v.version === familyData.currentVersion);
      if (currentVersion) {
        this.downloadVersion(currentVersion);
        this.messageService.add({
          severity: 'info',
          summary: 'Not in Revit',
          detail: 'Running outside Revit. Using download instead.',
        });
      }
    }
  }

  private subscribeToLoadResult(): void {
    const handler = (event: Event): void => {
      const customEvent = event as unknown as RevitFamilyLoadedDetail;
      const result = customEvent.detail;

      if (result.success) {
        this.messageService.add({
          severity: 'success',
          summary: 'Family Loaded',
          detail: `${result.familyName ?? 'Family'} loaded successfully`,
        });
      } else {
        this.messageService.add({
          severity: 'error',
          summary: 'Load Failed',
          detail: result.error ?? 'Unknown error',
        });
      }

      window.removeEventListener('revit:family-loaded', handler);
    };

    window.addEventListener('revit:family-loaded', handler);
  }

  protected isCurrentVersion(version: FamilyVersion): boolean {
    const familyData = this.family();
    if (!familyData) {
      return false;
    }
    return version.version === familyData.currentVersion;
  }

  // Type Catalog methods
  protected isTypeSelected(index: number): boolean {
    return this.selectedTypes().has(index);
  }

  protected toggleTypeSelection(index: number): void {
    this.selectedTypes.update(selected => {
      const newSet = new Set(selected);
      if (newSet.has(index)) {
        newSet.delete(index);
      } else {
        newSet.add(index);
      }
      return newSet;
    });
  }

  protected toggleAllTypes(): void {
    const types = this.typeCatalogTypes();
    const currentSelected = this.selectedTypes();

    if (currentSelected.size === types.length) {
      // Deselect all
      this.selectedTypes.set(new Set());
    } else {
      // Select all
      const allIndices = new Set(types.map((_, i) => i));
      this.selectedTypes.set(allIndices);
    }
  }

  protected loadSelectedTypes(): void {
    const familyData = this.family();
    if (!familyData?.typeCatalog) {
      return;
    }

    const selectedIndices = Array.from(this.selectedTypes());
    const selectedTypeEntries: TypeCatalogEntry[] = selectedIndices.map(
      i => familyData.typeCatalog!.types[i]
    );

    const revitBridge = window.revitBridge;
    if (revitBridge) {
      revitBridge.postMessage({
        type: 'ui:load-family-types',
        payload: {
          familyId: familyData.id,
          version: familyData.currentVersion,
          types: selectedTypeEntries,
        },
      });

      this.subscribeToLoadResult();
    } else {
      this.messageService.add({
        severity: 'info',
        summary: 'Not in Revit',
        detail: 'Running outside Revit. Cannot load types.',
      });
    }
  }

  // Pre-load summary dialog handlers

  /**
   * Handle pre-load dialog visibility change
   */
  protected onPreLoadDialogVisibleChange(visible: boolean): void {
    this.preLoadDialogVisible.set(visible);
    if (!visible) {
      this.preLoadData.set(null);
    }
  }

  /**
   * Handle load confirmation from pre-load summary dialog
   */
  protected onPreLoadConfirmed(): void {
    this.messageService.add({
      severity: 'info',
      summary: 'Loading Family',
      detail: 'Loading family with nested dependencies...',
    });
    this.preLoadDialogVisible.set(false);
    this.preLoadData.set(null);
  }

  /**
   * Handle load cancellation from pre-load summary dialog
   */
  protected onPreLoadCancelled(): void {
    this.preLoadDialogVisible.set(false);
    this.preLoadData.set(null);
  }
}
