import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { TabsModule } from 'primeng/tabs';
import { ToastModule } from 'primeng/toast';
import { BadgeModule } from 'primeng/badge';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { FamilyListComponent } from '../family-list/family-list.component';
import { DraftListComponent } from '../draft-list/draft-list.component';
import { LibraryStatusComponent } from '../library-status/library-status.component';
import { SystemTypeListComponent } from '../system-type-list/system-type-list.component';
import { DependenciesListComponent } from '../../../library/components/dependencies-list/dependencies-list.component';
import { RevitBridgeService } from '../../../../core/services/revit-bridge.service';

@Component({
  selector: 'app-queue',
  imports: [
    BadgeModule,
    ButtonModule,
    DialogModule,
    TabsModule,
    ToastModule,
    DependenciesListComponent,
    FamilyListComponent,
    DraftListComponent,
    LibraryStatusComponent,
    SystemTypeListComponent,
  ],
  templateUrl: './queue.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class QueueComponent {
  private readonly revitBridge = inject(RevitBridgeService);

  protected readonly activeTab = signal<string>('0');

  // Nested dependencies state
  protected readonly showDependenciesDialog = signal(false);

  // Computed from RevitBridge signals
  protected readonly nestedDetected = this.revitBridge.nestedDetected;
  protected readonly hasNestedDependencies = this.revitBridge.hasNestedDependencies;
  protected readonly unpublishedNestedCount = this.revitBridge.unpublishedNestedCount;

  protected readonly hasUnpublishedWarning = computed(() =>
    this.unpublishedNestedCount() > 0
  );

  constructor() {
    // Subscribe to nested detected events
    this.revitBridge.onNestedDetected()
      .pipe(takeUntilDestroyed())
      .subscribe(() => {
        this.showDependenciesDialog.set(true);
      });
  }

  protected closeDependenciesDialog(): void {
    this.showDependenciesDialog.set(false);
  }

  protected clearNestedDetected(): void {
    this.revitBridge.clearNestedDetected();
    this.showDependenciesDialog.set(false);
  }
}
