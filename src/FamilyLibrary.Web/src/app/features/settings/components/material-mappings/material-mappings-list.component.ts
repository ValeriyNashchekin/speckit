import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { MessageService } from 'primeng/api';
import { TableModule } from 'primeng/table';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';
import { MaterialMapping, CreateMaterialMappingRequest, UpdateMaterialMappingRequest } from '../../../../core/models';
import { ConfirmDialogService } from '../../../../shared/components/confirm-dialog/confirm-dialog.service';
import { MaterialMappingService } from '../../../../core/services/material-mapping.service';
import { MaterialMappingEditorComponent } from './material-mapping-editor/material-mapping-editor.component';

/**
 * Component for managing material mappings in settings.
 * Displays a table of mappings with create, edit, and delete actions.
 */
@Component({
  selector: 'app-material-mappings-list',
  imports: [
    ButtonModule,
    ConfirmDialogModule,
    TableModule,
    MaterialMappingEditorComponent,
    ToastModule,
    TooltipModule,
  ],
  templateUrl: './material-mappings-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MaterialMappingsListComponent {
  private readonly mappingService = inject(MaterialMappingService);
  private readonly confirmDialogService = inject(ConfirmDialogService);
  private readonly messageService = inject(MessageService);

  // State signals
  protected readonly mappings = signal<MaterialMapping[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly isDeleting = signal<string | null>(null);

  // Dialog state
  protected readonly editorVisible = signal(false);
  protected readonly editingMapping = signal<MaterialMapping | null>(null);

  constructor() {
    this.loadMappings();
  }

  private loadMappings(): void {
    this.isLoading.set(true);
    this.mappingService.getMappings().subscribe({
      next: (data) => {
        this.mappings.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load material mappings',
        });
        this.isLoading.set(false);
      },
    });
  }

  protected createMapping(): void {
    this.editingMapping.set(null);
    this.editorVisible.set(true);
  }

  protected editMapping(mapping: MaterialMapping): void {
    this.editingMapping.set(mapping);
    this.editorVisible.set(true);
  }

  protected onEditorClosed(): void {
    this.editorVisible.set(false);
    this.editingMapping.set(null);
  }

  protected onMappingSaved(request: CreateMaterialMappingRequest | UpdateMaterialMappingRequest): void {
    const isEdit = this.editingMapping() !== null;
    const mappingId = this.editingMapping()?.id;

    if (isEdit && mappingId) {
      this.mappingService.updateMapping(mappingId, request as UpdateMaterialMappingRequest).subscribe({
        next: (updated) => {
          this.mappings.update((items) =>
            items.map((m) => (m.id === mappingId ? updated : m))
          );
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Material mapping updated successfully',
          });
          this.editorVisible.set(false);
          this.editingMapping.set(null);
        },
        error: () => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to update material mapping',
          });
        },
      });
    } else {
      this.mappingService.createMapping(request as CreateMaterialMappingRequest).subscribe({
        next: (newMapping) => {
          this.mappings.update((items) => [...items, newMapping]);
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Material mapping created successfully',
          });
          this.editorVisible.set(false);
          this.editingMapping.set(null);
        },
        error: () => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to create material mapping',
          });
        },
      });
    }
  }

  protected deleteMapping(mapping: MaterialMapping, event: Event): void {
    this.confirmDialogService
      .delete(
        `Are you sure you want to delete mapping "${mapping.templateMaterialName}" -> "${mapping.projectMaterialName}"?`,
        event.currentTarget as EventTarget
      )
      .subscribe((confirmed) => {
        if (confirmed) {
          this.performDelete(mapping.id);
        }
      });
  }

  private performDelete(id: string): void {
    this.isDeleting.set(id);
    this.mappingService.deleteMapping(id).subscribe({
      next: () => {
        this.mappings.update((items) => items.filter((m) => m.id !== id));
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Material mapping deleted successfully',
        });
        this.isDeleting.set(null);
      },
      error: (error) => {
        const errorMessage =
          error?.error?.message || error?.message || 'Failed to delete material mapping';
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: errorMessage,
        });
        this.isDeleting.set(null);
      },
    });
  }

  protected refreshMappings(): void {
    this.loadMappings();
  }
}
