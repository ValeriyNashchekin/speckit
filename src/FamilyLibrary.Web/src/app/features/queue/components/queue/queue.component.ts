import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { TabsModule } from 'primeng/tabs';
import { ToastModule } from 'primeng/toast';
import { FamilyListComponent } from '../family-list/family-list.component';
import { DraftListComponent } from '../draft-list/draft-list.component';
import { LibraryStatusComponent } from '../library-status/library-status.component';

@Component({
  selector: 'app-queue',
  imports: [TabsModule, ToastModule, FamilyListComponent, DraftListComponent, LibraryStatusComponent],
  templateUrl: './queue.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class QueueComponent {
  protected readonly activeTab = signal<string>('0');
}
