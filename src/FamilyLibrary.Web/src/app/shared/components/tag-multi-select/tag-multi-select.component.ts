import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MultiSelectModule } from 'primeng/multiselect';
import { Tag } from '../../../core/models';

@Component({
  selector: 'app-tag-multi-select',
  imports: [FormsModule, MultiSelectModule],
  templateUrl: './tag-multi-select.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TagMultiSelectComponent {
  // Inputs
  readonly tags = input<Tag[]>([]);
  readonly selectedTagIds = input<string[]>([]);
  readonly placeholder = input<string>('Select tags');
  readonly disabled = input<boolean>(false);
  readonly readonly = input<boolean>(false);
  readonly maxSelectedLabels = input<number>(3);
  readonly showClear = input<boolean>(true);
  readonly fluid = input<boolean>(true);

  // Outputs
  readonly selectedTagIdsChange = output<string[]>();

  // Computed: map of tag id to tag for quick lookup
  protected readonly tagMap = computed(() => {
    const map = new Map<string, Tag>();
    for (const tag of this.tags()) {
      map.set(tag.id, tag);
    }
    return map;
  });

  // Computed: selected tag objects
  protected readonly selectedTags = computed(() => {
    const ids = this.selectedTagIds();
    const map = this.tagMap();
    return ids.map((id) => map.get(id)).filter((tag): tag is Tag => tag !== undefined);
  });

  protected onSelectionChange(selectedTags: Tag[]): void {
    const ids = selectedTags.map((tag) => tag.id);
    this.selectedTagIdsChange.emit(ids);
  }

  protected getTagColor(tag: Tag): string {
    return tag.color ?? '#6B7280';
  }
}
