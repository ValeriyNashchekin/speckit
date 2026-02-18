import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { ChangeItemComponent } from '../changelog/change-item.component';
import { ChangeItem } from '../../../../core/models/scanner.models';

export interface ChangeSet {
  changes: ChangeItem[];
}

@Component({
  selector: 'app-view-changes-modal',
  imports: [DialogModule, ButtonModule, ChangeItemComponent],
  templateUrl: './view-changes-modal.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ViewChangesModalComponent {
  // Inputs
  changes = input<ChangeSet | null>(null);
  familyName = input<string>('');
  visible = input<boolean>(false);

  // Outputs
  publish = output<void>();
  discard = output<void>();
  cancel = output<void>();
  visibleChange = output<boolean>();

  // Computed: list of changes from ChangeSet
  protected readonly changesList = computed(() => this.changes()?.changes ?? []);

  // Computed: count of changes
  protected readonly changeCount = computed(() => this.changesList().length);

  // Computed: has any changes
  protected readonly hasChanges = computed(() => this.changeCount() > 0);

  // Computed: dialog header with family name and count
  protected readonly dialogHeader = computed(() => {
    const name = this.familyName();
    const count = this.changeCount();
    const plural = count === 1 ? 'change' : 'changes';
    return name ? `${name} - ${count} ${plural}` : `${count} ${plural}`;
  });

  protected onPublish(): void {
    this.publish.emit();
    this.visibleChange.emit(false);
  }

  protected onDiscard(): void {
    this.discard.emit();
    this.visibleChange.emit(false);
  }

  protected onCancel(): void {
    this.cancel.emit();
    this.visibleChange.emit(false);
  }

  // Handle dialog hide
  protected onHide(): void {
    this.visibleChange.emit(false);
  }
}
