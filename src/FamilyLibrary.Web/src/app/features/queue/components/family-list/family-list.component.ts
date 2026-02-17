import { ChangeDetectionStrategy, Component, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { InputTextModule } from 'primeng/inputtext';
import { TableModule, TableLazyLoadEvent } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { QueueService } from '../../services/queue.service';
import { Family } from '../../../../core/models/family.model';
import { RolesService } from '../../../roles/services/roles.service';
import { FamilyRole } from '../../../../core/models/family-role.model';

@Component({
  selector: 'app-family-list',
  imports: [
    TableModule,
    TagModule,
    ButtonModule,
    InputTextModule,
    SelectModule,
    FormsModule,
    DatePipe,
  ],
  templateUrl: './family-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FamilyListComponent {
  private readonly queueService = inject(QueueService);
  private readonly rolesService = inject(RolesService);

  protected readonly families = signal<Family[]>([]);
  protected readonly roles = signal<FamilyRole[]>([]);
  protected readonly loading = signal(true);
  protected readonly totalRecords = signal(0);
  protected readonly searchTerm = signal('');
  protected readonly selectedRoleId = signal<string | null>(null);
  protected readonly rowsPerPage = signal(20);
  protected readonly first = signal(0);

  constructor() {
    this.loadRoles();
    this.loadFamilies();

    effect(() => {
      // Reload when filters change
      const search = this.searchTerm();
      const roleId = this.selectedRoleId();
      // Effect runs but actual reload is triggered via debounce
    });
  }

  private loadRoles(): void {
    this.rolesService.getRoles(1, 1000).subscribe({
      next: result => {
        this.roles.set(result.data);
      },
    });
  }

  protected loadFamilies(): void {
    this.loading.set(true);
    const page = Math.floor(this.first() / this.rowsPerPage()) + 1;

    this.queueService
      .getFamilies(page, this.rowsPerPage(), {
        searchTerm: this.searchTerm() || undefined,
        roleId: this.selectedRoleId(),
      })
      .subscribe({
        next: result => {
          this.families.set(result.data);
          this.totalRecords.set(result.totalCount);
          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
        },
      });
  }

  protected onLazyLoad(event: TableLazyLoadEvent): void {
    this.first.set(event.first ?? 0);
    this.rowsPerPage.set(event.rows ?? 20);
    this.loadFamilies();
  }

  protected onSearch(): void {
    this.first.set(0);
    this.loadFamilies();
  }

  protected onFilterChange(): void {
    this.first.set(0);
    this.loadFamilies();
  }

  protected clearFilters(): void {
    this.searchTerm.set('');
    this.selectedRoleId.set(null);
    this.first.set(0);
    this.loadFamilies();
  }

  protected getRoleName(roleId: string): string {
    const role = this.roles().find(r => r.id === roleId);
    return role?.name ?? 'Unknown';
  }
}
