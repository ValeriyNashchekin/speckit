import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { ChipModule } from 'primeng/chip';
import { TagModule } from 'primeng/tag';
import { ChangeItem, ChangeCategory } from '../../../../core/models/scanner.models';

@Component({
  selector: 'app-change-item',
  imports: [ChipModule, TagModule],
  templateUrl: './change-item.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChangeItemComponent {
  changeItem = input.required<ChangeItem>();

  protected readonly categoryConfig = computed(() => {
    const category = this.changeItem().category;
    return this.getCategoryConfig(category);
  });

  protected readonly hasValueChange = computed(() => {
    const item = this.changeItem();
    return item.previousValue !== undefined || item.currentValue !== undefined;
  });

  protected readonly hasListChange = computed(() => {
    const item = this.changeItem();
    const addedCount = item.addedItems?.length ?? 0;
    const removedCount = item.removedItems?.length ?? 0;
    return addedCount > 0 || removedCount > 0;
  });

  protected readonly hasParameterChanges = computed(() => {
    const item = this.changeItem();
    return (item.parameterChanges?.length ?? 0) > 0;
  });

  private getCategoryConfig(category: ChangeCategory): { icon: string; label: string; severity: 'secondary' | 'info' | 'success' | 'warn' | 'danger' | 'contrast' } {
    const configs: Record<ChangeCategory, { icon: string; label: string; severity: 'secondary' | 'info' | 'success' | 'warn' | 'danger' | 'contrast' }> = {
      Name: { icon: 'pi pi-pencil', label: 'Name', severity: 'info' },
      Category: { icon: 'pi pi-folder', label: 'Category', severity: 'info' },
      Types: { icon: 'pi pi-list', label: 'Types', severity: 'warn' },
      Parameters: { icon: 'pi pi-sliders-h', label: 'Parameters', severity: 'warn' },
      Geometry: { icon: 'pi pi-box', label: 'Geometry', severity: 'danger' },
      Txt: { icon: 'pi pi-file', label: 'Txt', severity: 'contrast' },
    };
    return configs[category];
  }
}
