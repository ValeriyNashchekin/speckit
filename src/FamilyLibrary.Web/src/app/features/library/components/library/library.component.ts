import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { PaginatorModule, PaginatorState } from 'primeng/paginator';
import { SelectButtonModule } from 'primeng/selectbutton';
import { Family, FamilyListRequest } from '../../../../core/models/family.model';
import { FamilyCardComponent } from '../family-card/family-card.component';
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
    CardModule,
    FamilyCardComponent,
    DatePipe,
  ],
  templateUrl: './library.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LibraryComponent {
  private readonly libraryService = inject(LibraryService);

  // State
  protected readonly families = signal<Family[]>([]);
  protected readonly loading = signal(true);
  protected readonly totalRecords = signal(0);
  protected readonly rowsPerPage = signal(20);
  protected readonly first = signal(0);

  // View mode
  protected readonly viewMode = signal<'cards' | 'table'>('cards');
  protected readonly viewModeOptions: ViewModeOption[] = [
    { label: 'Cards', value: 'cards', icon: 'pi pi-th-large' },
    { label: 'Table', value: 'table', icon: 'pi pi-list' },
  ];

  // Filters
  protected readonly searchTerm = signal('');
  protected readonly selectedRoleId = signal<string | null>(null);
  protected readonly selectedCategoryId = signal<string | null>(null);

  protected loadFamilies(): void {
    this.loading.set(true);

    const page = Math.floor(this.first() / this.rowsPerPage()) + 1;
    const filters: FamilyListRequest = {
      searchTerm: this.searchTerm() || undefined,
      roleId: this.selectedRoleId(),
      categoryId: this.selectedCategoryId(),
    };

    this.libraryService.getFamilies(page, this.rowsPerPage(), filters).subscribe({
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

  protected onSearchInputChange(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.searchTerm.set(target.value);
  }

  protected onRoleChange(event: Event): void {
    const target = event.target as HTMLSelectElement;
    this.selectedRoleId.set(target.value || null);
  }

  protected onCategoryChange(event: Event): void {
    const target = event.target as HTMLSelectElement;
    this.selectedCategoryId.set(target.value || null);
  }

  protected applyFilters(): void {
    this.first.set(0);
    this.loadFamilies();
  }

  protected onFamilySelect(family: Family): void {
    // TODO: Navigate to family details or open detail dialog
    console.log('Selected family:', family);
  }
}
