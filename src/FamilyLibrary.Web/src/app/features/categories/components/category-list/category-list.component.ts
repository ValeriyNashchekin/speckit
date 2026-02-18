import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { InputTextModule } from 'primeng/inputtext';
import { MessageService } from 'primeng/api';
import { TableModule } from 'primeng/table';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';
import { Category, CreateCategoryRequest, UpdateCategoryRequest } from '../../../../core/models';
import { ConfirmDialogService } from '../../../../shared/components/confirm-dialog/confirm-dialog.service';
import { CategoriesService } from '../../services/categories.service';
import { CategoryEditorComponent } from '../category-editor/category-editor.component';

@Component({
  selector: 'app-category-list',
  imports: [
    ButtonModule,
    CategoryEditorComponent,
    ConfirmDialogModule,
    FormsModule,
    InputTextModule,
    TableModule,
    ToastModule,
    TooltipModule,
  ],
  templateUrl: './category-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CategoryListComponent {
  private readonly categoriesService = inject(CategoriesService);
  private readonly confirmDialogService = inject(ConfirmDialogService);
  private readonly messageService = inject(MessageService);

  // Data state
  protected readonly categories = signal<Category[]>([]);
  protected readonly isLoading = signal(false);

  // Deletion state
  protected readonly isDeleting = signal<string | null>(null);

  // Dialog state
  protected readonly editorVisible = signal(false);
  protected readonly editingCategory = signal<Category | null>(null);

  constructor() {
    this.loadCategories();
  }

  private loadCategories(): void {
    this.isLoading.set(true);
    this.categoriesService.getCategories().subscribe({
      next: data => {
        this.categories.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load categories',
        });
        this.isLoading.set(false);
      },
    });
  }

  protected createCategory(): void {
    this.editingCategory.set(null);
    this.editorVisible.set(true);
  }

  protected editCategory(category: Category): void {
    this.editingCategory.set(category);
    this.editorVisible.set(true);
  }

  protected onEditorClosed(): void {
    this.editorVisible.set(false);
    this.editingCategory.set(null);
  }

  protected onCategorySaved(request: CreateCategoryRequest | UpdateCategoryRequest): void {
    const isEdit = this.editingCategory() !== null;

    if (isEdit && this.editingCategory()) {
      this.categoriesService
        .updateCategory(this.editingCategory()!.id, request as UpdateCategoryRequest)
        .subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Success',
              detail: 'Category updated successfully',
            });
            this.editorVisible.set(false);
            this.editingCategory.set(null);
            this.loadCategories();
          },
          error: () => {
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: 'Failed to update category',
            });
          },
        });
    } else {
      this.categoriesService.createCategory(request as CreateCategoryRequest).subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Category created successfully',
          });
          this.editorVisible.set(false);
          this.editingCategory.set(null);
          this.loadCategories();
        },
        error: () => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to create category',
          });
        },
      });
    }
  }

  protected deleteCategory(category: Category, event: Event): void {
    this.confirmDialogService
      .delete(`Are you sure you want to delete category "${category.name}"?`, event.currentTarget as EventTarget)
      .subscribe(confirmed => {
        if (confirmed) {
          this.performDelete(category);
        }
      });
  }

  private performDelete(category: Category): void {
    this.isDeleting.set(category.id);
    this.categoriesService.deleteCategory(category.id).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Category deleted successfully',
        });
        this.isDeleting.set(null);
        this.loadCategories();
      },
      error: (error: { error?: { message?: string } }) => {
        this.isDeleting.set(null);
        const errorMessage = error?.error?.message ?? 'Failed to delete category';
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: errorMessage,
        });
      },
    });
  }

  protected refreshCategories(): void {
    this.loadCategories();
  }
}
