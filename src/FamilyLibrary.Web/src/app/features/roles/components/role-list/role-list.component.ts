import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { InputTextModule } from 'primeng/inputtext';
import { MessageService } from 'primeng/api';
import { SelectModule } from 'primeng/select';
import { TableModule, TableLazyLoadEvent } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';
import { Category, FamilyRole, CreateFamilyRoleRequest, Tag, UpdateFamilyRoleRequest } from '../../../../core/models';
import { RoleType } from '../../../../core/models/family-role.model';
import { ConfirmDialogService } from '../../../../shared/components/confirm-dialog/confirm-dialog.service';
import { CategoriesService } from '../../../categories/services/categories.service';
import { TagsService } from '../../../tags/services/tags.service';
import { RolesStore } from '../../roles.store';
import { RoleEditorComponent } from '../role-editor/role-editor.component';

@Component({
  selector: 'app-role-list',
  imports: [
    ButtonModule,
    ConfirmDialogModule,
    FormsModule,
    InputTextModule,
    RoleEditorComponent,
    SelectModule,
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
  private readonly categoriesService = inject(CategoriesService);
  private readonly tagsService = inject(TagsService);

  // Pagination state
  protected readonly first = signal(0);
  protected readonly rows = signal(10);

  protected readonly searchTerm = signal('');
  protected readonly selectedType = signal<RoleType | null>(null);
  protected readonly typeOptions: { label: string; value: RoleType }[] = [
    { label: 'Loadable', value: 'Loadable' },
    { label: 'System', value: 'System' },
  ];

  // Deletion state
  protected readonly isDeleting = signal<string | null>(null);

  // Dialog state
  protected readonly editorVisible = signal(false);
  protected readonly editingRole = signal<FamilyRole | null>(null);

  // Reference data
  protected readonly categories = signal<Category[]>([]);
  protected readonly tags = signal<Tag[]>([]);

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
    // Load reference data
    this.loadCategories();
    this.loadTags();
  }

  private loadCategories(): void {
    this.categoriesService.getCategories().subscribe({
      next: (data) => this.categories.set(data),
      error: (err) => console.error('Failed to load categories:', err),
    });
  }

  private loadTags(): void {
    this.tagsService.getTags().subscribe({
      next: (data) => this.tags.set(data),
      error: (err) => console.error('Failed to load tags:', err),
    });
  }

  private loadRoles(): void {
    this.rolesStore.loadRoles({
      page: Math.floor(this.first() / this.rows()) + 1,
      pageSize: this.rows(),
      searchTerm: this.searchTerm() || undefined,
      type: this.selectedType() || undefined,
    });
  }

  protected onTypeFilterChange(type: RoleType | null): void {
    this.selectedType.set(type);
    this.first.set(0);
    this.loadRoles();
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
    this.selectedType.set(null);
    this.first.set(0);
    this.loadRoles();
  }

  protected createRole(): void {
    this.editingRole.set(null);
    this.editorVisible.set(true);
  }

  protected editRole(role: FamilyRole): void {
    this.editingRole.set(role);
    this.editorVisible.set(true);
  }

  protected onEditorClosed(): void {
    this.editorVisible.set(false);
    this.editingRole.set(null);
  }

  protected async onRoleSaved(request: CreateFamilyRoleRequest | UpdateFamilyRoleRequest): Promise<void> {
    const isEdit = this.editingRole() !== null;
    let success: boolean;

    if (isEdit && this.editingRole()) {
      success = !!(await this.rolesStore.updateRole(this.editingRole()!.id, request as UpdateFamilyRoleRequest));
    } else {
      success = !!(await this.rolesStore.createRole(request as CreateFamilyRoleRequest));
    }

    if (success) {
      this.messageService.add({
        severity: 'success',
        summary: 'Success',
        detail: isEdit ? 'Role updated successfully' : 'Role created successfully',
      });
      this.editorVisible.set(false);
      this.editingRole.set(null);
    } else {
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: isEdit ? 'Failed to update role' : 'Failed to create role',
      });
    }
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

  protected getCategoryName(categoryId: string | null): string {
    if (!categoryId) return '-';
    const cat = this.categories().find(c => c.id === categoryId);
    return cat?.name ?? categoryId;
  }
}
