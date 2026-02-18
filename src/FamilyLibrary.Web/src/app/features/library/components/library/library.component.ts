import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { PaginatorModule, PaginatorState } from 'primeng/paginator';
import { SelectButtonModule } from 'primeng/selectbutton';
import { take } from 'rxjs';
import { Category } from '../../../../core/models/category.model';
import { Family, FamilyListRequest } from '../../../../core/models/family.model';
import { FamilyRole } from '../../../../core/models/family-role.model';
import { Tag } from '../../../../core/models/tag.model';
import { CategoriesService } from '../../../categories/services/categories.service';
import { RolesService } from '../../../roles/services/roles.service';
import { TagsService } from '../../../tags/services/tags.service';
import { FamilyCardComponent } from '../family-card/family-card.component';
import { LibraryFiltersComponent } from '../library-filters/library-filters.component';
import { LibraryService, PagedResult } from '../../services/library.service';

interface ViewModeOption {
  label: string;
  value: 'cards' | 'table';
  icon: string;
}

@Component({
  selector: 'app-library',
  imports: [
    FormsModule,
    ButtonModule,
    SelectButtonModule,
    PaginatorModule,
    FamilyCardComponent,
    LibraryFiltersComponent,
    DatePipe,
  ],
  templateUrl: './library.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LibraryComponent {
  private readonly libraryService = inject(LibraryService);
  private readonly rolesService = inject(RolesService);
  private readonly categoriesService = inject(CategoriesService);
  private readonly tagsService = inject(TagsService);

  // State
  protected readonly families = signal<Family[]>([]);
  protected readonly loading = signal(true);
  protected readonly totalRecords = signal(0);
  protected readonly rowsPerPage = signal(20);
  protected readonly first = signal(0);

  // Current filters (stored for pagination)
  protected currentFilters = signal<FamilyListRequest>({});

  // Filter data for library-filters component
  protected readonly roles = signal<FamilyRole[]>([]);
  protected readonly categories = signal<Category[]>([]);
  protected readonly tags = signal<Tag[]>([]);

  // View mode
  protected readonly viewMode = signal<'cards' | 'table'>('cards');
  protected readonly viewModeOptions: ViewModeOption[] = [
    { label: 'Cards', value: 'cards', icon: 'pi pi-th-large' },
    { label: 'Table', value: 'table', icon: 'pi pi-list' },
  ];

  constructor() {
    this.loadFilterData();
    this.loadFamilies();
  }

  private loadFilterData(): void {
    // Load roles (first page, large page size to get all)
    this.rolesService.getRoles(1, 1000).pipe(take(1)).subscribe({
      next: (result) => this.roles.set(result.data),
      error: () => this.roles.set([]),
    });

    // Load categories
    this.categoriesService.getCategories().pipe(take(1)).subscribe({
      next: (categories) => this.categories.set(categories),
      error: () => this.categories.set([]),
    });

    // Load tags
    this.tagsService.getTags().pipe(take(1)).subscribe({
      next: (tags) => this.tags.set(tags),
      error: () => this.tags.set([]),
    });
  }

  protected loadFamilies(filters?: FamilyListRequest): void {
    this.loading.set(true);

    const page = Math.floor(this.first() / this.rowsPerPage()) + 1;
    const activeFilters = filters ?? this.currentFilters();

    this.libraryService.getFamilies(page, this.rowsPerPage(), activeFilters).subscribe({
      next: (result: PagedResult<Family>) => {
        this.families.set(result.data);
        this.totalRecords.set(result.totalCount);
        this.loading.set(false);
      },
      error: () => {
        this.families.set([]);
        this.totalRecords.set(0);
        this.loading.set(false);
      },
    });
  }

  protected onPageChange(event: PaginatorState): void {
    this.first.set(event.first ?? 0);
    this.rowsPerPage.set(event.rows ?? 20);
    this.loadFamilies();
  }

  protected onFilterChange(filters: FamilyListRequest): void {
    this.first.set(0);
    this.currentFilters.set(filters);
    this.loadFamilies(filters);
  }

  protected onFamilySelect(family: Family): void {
    // TODO: Navigate to family details or open detail dialog
    console.log('Selected family:', family);
  }
}
