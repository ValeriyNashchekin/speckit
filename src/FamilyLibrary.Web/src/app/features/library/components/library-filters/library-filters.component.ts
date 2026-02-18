import { ChangeDetectionStrategy, Component, effect, input, model, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { MultiSelectModule } from 'primeng/multiselect';
import { SelectModule } from 'primeng/select';
import { TooltipModule } from 'primeng/tooltip';
import { Category } from '../../../../core/models/category.model';
import { FamilyRole } from '../../../../core/models/family-role.model';
import { Tag } from '../../../../core/models/tag.model';
import { FamilyListRequest } from '../../../../core/models/family.model';

@Component({
  selector: 'app-library-filters',
  imports: [ButtonModule, FormsModule, InputTextModule, MultiSelectModule, SelectModule, TooltipModule],
  templateUrl: './library-filters.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LibraryFiltersComponent {
  // Two-way bindings for filter values
  searchTerm = model<string>('');
  selectedRoleId = model<string | null>(null);
  selectedCategoryId = model<string | null>(null);
  selectedTagIds = model<string[]>([]);

  // Input data for dropdowns
  roles = input<FamilyRole[]>([]);
  categories = input<Category[]>([]);
  tags = input<Tag[]>([]);

  // Outputs
  filterChange = output<FamilyListRequest>();

  // Debounce state
  protected readonly searchInputValue = signal('');

  // Computed options for selects
  protected readonly roleOptions = signal<{ label: string; value: string }[]>([]);
  protected readonly categoryOptions = signal<{ label: string; value: string }[]>([]);
  protected readonly tagOptions = signal<{ label: string; value: string }[]>([]);

  constructor() {
    // Update options when input data changes
    effect(() => {
      const roles = this.roles();
      this.roleOptions.set(
        roles.map(role => ({ label: role.name, value: role.id }))
      );
    });

    effect(() => {
      const categories = this.categories();
      this.categoryOptions.set(
        categories.map(category => ({ label: category.name, value: category.id }))
      );
    });

    effect(() => {
      const tags = this.tags();
      this.tagOptions.set(
        tags.map(tag => ({ label: tag.name, value: tag.id }))
      );
    });
  }

  protected onSearchInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.searchInputValue.set(input.value);
  }

  protected onSearch(): void {
    this.searchTerm.set(this.searchInputValue());
    this.emitFilterChange();
  }

  protected onRoleChange(value: string | null): void {
    this.selectedRoleId.set(value);
    this.emitFilterChange();
  }

  protected onCategoryChange(value: string | null): void {
    this.selectedCategoryId.set(value);
    this.emitFilterChange();
  }

  protected onTagsChange(value: string[]): void {
    this.selectedTagIds.set(value);
    this.emitFilterChange();
  }

  protected onClear(): void {
    this.searchTerm.set('');
    this.searchInputValue.set('');
    this.selectedRoleId.set(null);
    this.selectedCategoryId.set(null);
    this.selectedTagIds.set([]);
    this.emitFilterChange();
  }

  protected hasActiveFilters(): boolean {
    return !!(
      this.searchTerm() ||
      this.selectedRoleId() ||
      this.selectedCategoryId() ||
      this.selectedTagIds().length > 0
    );
  }

  private emitFilterChange(): void {
    const filters: FamilyListRequest = {};

    if (this.searchTerm()) {
      filters.searchTerm = this.searchTerm();
    }
    if (this.selectedRoleId()) {
      filters.roleId = this.selectedRoleId();
    }
    if (this.selectedCategoryId()) {
      filters.categoryId = this.selectedCategoryId();
    }
    if (this.selectedTagIds().length > 0) {
      filters.tagIds = this.selectedTagIds();
    }

    this.filterChange.emit(filters);
  }
}
