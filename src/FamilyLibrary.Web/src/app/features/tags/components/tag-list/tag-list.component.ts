import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { InputTextModule } from 'primeng/inputtext';
import { MessageService } from 'primeng/api';
import { TableModule } from 'primeng/table';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';
import { Tag, CreateTagRequest, UpdateTagRequest } from '../../../../core/models';
import { ConfirmDialogService } from '../../../../shared/components/confirm-dialog/confirm-dialog.service';
import { TagsService } from '../../services/tags.service';
import { TagEditorComponent } from '../tag-editor/tag-editor.component';

@Component({
  selector: 'app-tag-list',
  imports: [
    ButtonModule,
    ConfirmDialogModule,
    FormsModule,
    InputTextModule,
    TableModule,
    TagEditorComponent,
    ToastModule,
    TooltipModule,
  ],
  templateUrl: './tag-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TagListComponent {
  private readonly tagsService = inject(TagsService);
  private readonly confirmDialogService = inject(ConfirmDialogService);
  private readonly messageService = inject(MessageService);

  // State signals
  protected readonly tags = signal<Tag[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly isDeleting = signal<string | null>(null);

  // Dialog state
  protected readonly editorVisible = signal(false);
  protected readonly editingTag = signal<Tag | null>(null);

  constructor() {
    this.loadTags();
  }

  private loadTags(): void {
    this.isLoading.set(true);
    this.tagsService.getTags().subscribe({
      next: (data) => {
        this.tags.set(data);
        this.isLoading.set(false);
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load tags',
        });
        this.isLoading.set(false);
      },
    });
  }

  protected createTag(): void {
    this.editingTag.set(null);
    this.editorVisible.set(true);
  }

  protected editTag(tag: Tag): void {
    this.editingTag.set(tag);
    this.editorVisible.set(true);
  }

  protected onEditorClosed(): void {
    this.editorVisible.set(false);
    this.editingTag.set(null);
  }

  protected onTagSaved(request: CreateTagRequest | UpdateTagRequest): void {
    const isEdit = this.editingTag() !== null;
    const tagId = this.editingTag()?.id;

    if (isEdit && tagId) {
      this.tagsService.updateTag(tagId, request as UpdateTagRequest).subscribe({
        next: (updatedTag) => {
          this.tags.update((tags) => tags.map((t) => (t.id === tagId ? updatedTag : t)));
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Tag updated successfully',
          });
          this.editorVisible.set(false);
          this.editingTag.set(null);
        },
        error: (error) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to update tag',
          });
        },
      });
    } else {
      this.tagsService.createTag(request as CreateTagRequest).subscribe({
        next: (newTag) => {
          this.tags.update((tags) => [...tags, newTag]);
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Tag created successfully',
          });
          this.editorVisible.set(false);
          this.editingTag.set(null);
        },
        error: (error) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to create tag',
          });
        },
      });
    }
  }

  protected deleteTag(tag: Tag, event: Event): void {
    this.confirmDialogService
      .delete(`Are you sure you want to delete tag "${tag.name}"?`, event.currentTarget as EventTarget)
      .subscribe((confirmed) => {
        if (confirmed) {
          this.performDelete(tag.id);
        }
      });
  }

  private performDelete(id: string): void {
    this.isDeleting.set(id);
    this.tagsService.deleteTag(id).subscribe({
      next: () => {
        this.tags.update((tags) => tags.filter((t) => t.id !== id));
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Tag deleted successfully',
        });
        this.isDeleting.set(null);
      },
      error: (error) => {
        // Check if error indicates tag is in use
        const errorMessage = error?.error?.message || error?.message || 'Failed to delete tag';
        const isInUse = errorMessage.toLowerCase().includes('in use') || 
                        errorMessage.toLowerCase().includes('reference') ||
                        errorMessage.toLowerCase().includes('constraint');
        
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: isInUse ? 'Cannot delete tag: it is currently in use by families' : errorMessage,
        });
        this.isDeleting.set(null);
      },
    });
  }

  protected refreshTags(): void {
    this.loadTags();
  }
}
