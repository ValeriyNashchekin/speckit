import { ChangeDetectionStrategy, Component, computed, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { PanelModule } from 'primeng/panel';
import { ChangeSet, ChangeItem, ChangeCategory } from '../../../core/models/scanner.models';
import { ChangeItemComponent } from '../../library/components/changelog/change-item.component';

/**
 * Preview dialog for confirming family updates before applying changes.
 * Displays changes grouped by category with expandable details.
 */
@Component({
  selector: 'app-pre-update-preview',
  imports: [CommonModule, DialogModule, ButtonModule, TagModule, PanelModule, ChangeItemComponent],
  templateUrl: './pre-update-preview.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PreUpdatePreviewComponent {
  // Inputs
  visible = input<boolean>(false);
  changes = input<ChangeSet>({ items: [], hasChanges: false });
  familyName = input<string>('');

  // Outputs
  confirm = output<void>();
  cancel = output<void>();

  // Internal visibility signal for two-way binding
  protected readonly dialogVisible = signal(false);

  // Category display labels
  private readonly categoryLabels: Record<ChangeCategory, string> = {
    Name: 'Name',
    Category: 'Category',
    Types: 'Types',
    Parameters: 'Parameters',
    Geometry: 'Geometry',
    Txt: 'TXT Parameters',
  };

  // Category order for display
  private readonly categoryOrder: ChangeCategory[] = [
    'Name',
    'Category',
    'Types',
    'Parameters',
    'Geometry',
    'Txt',
  ];

  // Computed: group changes by category
  protected readonly groupedChanges = computed(() => {
    const items = this.changes().items;
    const groups = new Map<ChangeCategory, ChangeItem[]>();

    // Initialize all categories
    for (const category of this.categoryOrder) {
      groups.set(category, []);
    }

    // Group items
    for (const item of items) {
      const existing = groups.get(item.category) ?? [];
      existing.push(item);
      groups.set(item.category, existing);
    }

    // Return only non-empty groups in order
    return this.categoryOrder
      .filter((cat) => (groups.get(cat)?.length ?? 0) > 0)
      .map((cat) => ({
        category: cat,
        label: this.categoryLabels[cat],
        items: groups.get(cat) ?? [],
      }));
  });

  // Computed: count of affected types
  protected readonly affectedTypesCount = computed(() => {
    const typesCategory = this.changes().items.find((item) => item.category === 'Types');
    if (!typesCategory) return 0;

    const added = typesCategory.addedItems?.length ?? 0;
    const removed = typesCategory.removedItems?.length ?? 0;
    return added + removed;
  });

  // Computed: total changes count
  protected readonly totalChangesCount = computed(() => this.changes().items.length);

  // Computed: has any changes
  protected readonly hasChanges = computed(() => this.changes().hasChanges);

  /**
   * Get category tag severity
   */
  protected getCategorySeverity(category: ChangeCategory): 'info' | 'warn' | 'success' | 'secondary' {
    const severities: Record<ChangeCategory, 'info' | 'warn' | 'success' | 'secondary'> = {
      Name: 'info',
      Category: 'info',
      Types: 'warn',
      Parameters: 'secondary',
      Geometry: 'success',
      Txt: 'secondary',
    };
    return severities[category];
  }

  /**
   * Track by category
   */
  protected trackByCategory(_: number, group: { category: ChangeCategory }): string {
    return group.category;
  }

  /**
   * Handle confirm action
   */
  protected onConfirm(): void {
    this.confirm.emit();
    this.dialogVisible.set(false);
  }

  /**
   * Handle cancel action
   */
  protected onCancel(): void {
    this.cancel.emit();
    this.dialogVisible.set(false);
  }

  /**
   * Handle dialog visibility change
   */
  protected onVisibleChange(value: boolean): void {
    this.dialogVisible.set(value);
    if (!value) {
      this.cancel.emit();
    }
  }
}
