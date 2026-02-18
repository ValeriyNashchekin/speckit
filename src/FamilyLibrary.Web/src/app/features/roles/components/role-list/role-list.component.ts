import { ChangeDetectionStrategy, Component, inject, signal, computed, effect } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { InputTextModule } from 'primeng/inputtext';
import { MessageService } from 'primeng/api';
import { TableModule, TableLazyLoadEvent } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';
import { FamilyRole } from '../../../../core/models/family-role.model';
import { ConfirmDialogService } from '../../../../shared/components/confirm-dialog/confirm-dialog.service';
import { RolesStore } from '../../roles.store';

@Component({
  selector: 'app-role-list',
  imports: [
    ButtonModule,
    ConfirmDialogModule,
    FormsModule,
    InputTextModule,
    TableModule,
    TagModule,
    ToastModule,
    TooltipModule,
  ],
  templateUrl: './role-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RoleListComponent {
  private readonly rolesStore = inject(RolesStore);
  private readonly confirmDialogService = inject(ConfirmDialogService);
  private readonly messageService = inject(MessageService);

  // Pagination state
  protected readonly first = signal(0);
  protected readonly rows = signal(10);

  // Filter state
  protected readonly searchTerm = signal('');

  // Deletion state
  protected readonly isDeleting = signal<string | null>(null);

  // Store signals
  protected readonly roles = this.rolesStore.roles;
  protected readonly totalCount = this.rolesStore.totalCount;
  protected readonly isLoading = this.rolesStore.isLoading;
  protected readonly hasRoles = this.rolesStore.hasRoles;

  // Table config
  protected readonly rowsPerPageOptions = [10, 20, 50, 100];

  constructor() {
    // Load roles on init
    this.loadRoles();
  }

  private loadRoles(): void {
    this.rolesStore.loadRoles({
      page: Math.floor(this.first() / this.rows()) + 1,
      pageSize: this.rows(),
      searchTerm: this.searchTerm() || undefined,
    });
  }

  // Event handlers
  protected onPageChange(event: TableLazyLoadEvent): void {
    const first = event.first ?? 0;
    const rows = event.rows ?? 10;
    this.first.set(first);
    this.rows.set(rows);
    this.loadRoles();
  }

  protected onSearch(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.searchTerm.set(input.value);
    this.first.set(0);
    this.loadRoles();
  }

  protected clearSearch(): void {
    this.searchTerm.set('');
    this.first.set(0);
    this.loadRoles();
  }

  protected createRole(): void {
    this.messageService.add({
      severity: 'info',
      summary: 'Coming Soon',
      detail: 'Role creation will be implemented soon',
    });
  }

  protected editRole(role: FamilyRole): void {
    this.messageService.add({
      severity: 'info',
      summary: 'Coming Soon',
      detail: `Edit role "${role.name}" will be implemented soon`,
    });
  }

  protected deleteRole(role: FamilyRole, event: Event): void {
    this.confirmDialogService
      .delete(`Are you sure you want to delete role "${role.name}"?`, event.currentTarget as EventTarget)
      .subscribe(confirmed => {
        if (confirmed) {
          this.performDelete(role.id);
        }
      });
  }

  private async performDelete(id: string): Promise<void> {
    this.isDeleting.set(id);
    const success = await this.rolesStore.deleteRole(id);
    this.isDeleting.set(null);

    if (success) {
      this.messageService.add({
        severity: 'success',
        summary: 'Success',
        detail: 'Role deleted successfully',
      });
    } else {
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Failed to delete role',
      });
    }
  }

  protected refreshRoles(): void {
    this.loadRoles();
  }

  protected getTagSeverity(type: 'Loadable' | 'System'): 'info' | 'warn' {
    return type === 'Loadable' ? 'info' : 'warn';
  }
}
