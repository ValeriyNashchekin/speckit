import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { InputTextModule } from 'primeng/inputtext';
import { TableModule, TableLazyLoadEvent } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService, MessageService } from 'primeng/api';
import { QueueService } from '../../services/queue.service';
import { Draft, DraftStatus } from '../../../../core/models/draft.model';
import { RolesService } from '../../../roles/services/roles.service';
import { FamilyRole } from '../../../../core/models/family-role.model';

type TagSeverity = 'secondary' | 'info' | 'warn' | 'success' | 'danger';

const STATUS_SEVERITY: Record<DraftStatus, TagSeverity> = {
  'New': 'secondary',
  'RoleSelected': 'info',
  'Stamped': 'warn',
  'Published': 'success',
};

const STATUS_LABEL: Record<DraftStatus, string> = {
  'New': 'Pending',
  'RoleSelected': 'Ready',
  'Stamped': 'Stamped',
  'Published': 'Published',
};

@Component({
  selector: 'app-draft-list',
  imports: [
    TableModule,
    TagModule,
    ButtonModule,
    InputTextModule,
    SelectModule,
    FormsModule,
    TooltipModule,
    ConfirmDialogModule,
    DatePipe,
  ],
  templateUrl: './draft-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [ConfirmationService],
})
export class DraftListComponent {
  private readonly queueService = inject(QueueService);
  private readonly rolesService = inject(RolesService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly messageService = inject(MessageService);

  protected readonly drafts = signal<Draft[]>([]);
  protected readonly roles = signal<FamilyRole[]>([]);
  protected readonly loading = signal(true);
  protected readonly totalRecords = signal(0);
  protected readonly searchTerm = signal('');
  protected readonly selectedStatus = signal<DraftStatus | null>(null);
  protected readonly rowsPerPage = signal(20);
  protected readonly first = signal(0);
  protected readonly selectedDrafts = signal<Draft[]>([]);

  protected readonly statusOptions: { label: string; value: DraftStatus }[] = [
    { label: 'New', value: 'New' },
    { label: 'Role Selected', value: 'RoleSelected' },
    { label: 'Stamped', value: 'Stamped' },
    { label: 'Published', value: 'Published' },
  ];

  constructor() {
    this.loadRoles();
    this.loadDrafts();
  }

  private loadRoles(): void {
    this.rolesService.getRoles(1, 1000).subscribe({
      next: result => {
        this.roles.set(result.data);
      },
    });
  }

  protected loadDrafts(): void {
    this.loading.set(true);
    const page = Math.floor(this.first() / this.rowsPerPage()) + 1;

    this.queueService
      .getDrafts(page, this.rowsPerPage(), {
        searchTerm: this.searchTerm() || undefined,
        status: this.selectedStatus(),
      })
      .subscribe({
        next: result => {
          this.drafts.set(result.data);
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
    this.loadDrafts();
  }

  protected onSearch(): void {
    this.first.set(0);
    this.loadDrafts();
  }

  protected onFilterChange(): void {
    this.first.set(0);
    this.loadDrafts();
  }

  protected clearFilters(): void {
    this.searchTerm.set('');
    this.selectedStatus.set(null);
    this.first.set(0);
    this.loadDrafts();
  }

  protected getStatusSeverity(status: DraftStatus): TagSeverity {
    return STATUS_SEVERITY[status] ?? 'secondary';
  }

  protected getStatusLabel(status: DraftStatus): string {
    return STATUS_LABEL[status] ?? status;
  }

  protected getRoleName(roleId: string | null): string {
    if (!roleId) return 'Not selected';
    const role = this.roles().find(r => r.id === roleId);
    return role?.name ?? 'Unknown';
  }

  protected onRoleSelect(draft: Draft, roleId: string): void {
    this.queueService.updateDraft(draft.id, { selectedRoleId: roleId }).subscribe({
      next: updated => {
        const currentDrafts = this.drafts();
        const index = currentDrafts.findIndex(d => d.id === draft.id);
        if (index !== -1) {
          currentDrafts[index] = updated;
          this.drafts.set([...currentDrafts]);
        }
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Role updated successfully',
        });
      },
      error: () => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to update role',
        });
      },
    });
  }

  protected stampDraft(draft: Draft): void {
    this.messageService.add({
      severity: 'info',
      summary: 'Stamp',
      detail: `Stamping ${draft.familyName}...`,
    });
  }

  protected publishDraft(draft: Draft): void {
    this.confirmationService.confirm({
      message: `Are you sure you want to publish "${draft.familyName}"?`,
      header: 'Confirm Publish',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Publish',
      rejectLabel: 'Cancel',
      accept: () => {
        this.queueService.batchPublish({ draftIds: [draft.id] }).subscribe({
          next: result => {
            if (result.successful > 0) {
              this.messageService.add({
                severity: 'success',
                summary: 'Success',
                detail: 'Family published successfully',
              });
              this.loadDrafts();
            } else {
              this.messageService.add({
                severity: 'error',
                summary: 'Error',
                detail: result.errors.join(', ') || 'Failed to publish family',
              });
            }
          },
        });
      },
    });
  }

  protected deleteDraft(draft: Draft): void {
    this.confirmationService.confirm({
      message: `Are you sure you want to delete "${draft.familyName}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Delete',
      rejectLabel: 'Cancel',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.queueService.deleteDraft(draft.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Success',
              detail: 'Draft deleted successfully',
            });
            this.loadDrafts();
          },
          error: () => {
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: 'Failed to delete draft',
            });
          },
        });
      },
    });
  }

  protected batchPublish(): void {
    const selected = this.selectedDrafts();
    if (selected.length === 0) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Please select drafts to publish',
      });
      return;
    }

    this.confirmationService.confirm({
      message: `Are you sure you want to publish ${selected.length} families?`,
      header: 'Confirm Batch Publish',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Publish All',
      rejectLabel: 'Cancel',
      accept: () => {
        this.queueService
          .batchPublish({ draftIds: selected.map(d => d.id) })
          .subscribe({
            next: result => {
              this.messageService.add({
                severity: 'success',
                summary: 'Batch Publish Complete',
                detail: `${result.successful} published, ${result.failed} failed`,
              });
              this.selectedDrafts.set([]);
              this.loadDrafts();
            },
          });
      },
    });
  }
}
