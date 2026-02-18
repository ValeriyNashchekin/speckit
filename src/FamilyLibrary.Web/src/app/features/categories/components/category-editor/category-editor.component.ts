import { ChangeDetectionStrategy, Component, computed, effect, inject, input, output, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';

import { Category, CreateCategoryRequest, UpdateCategoryRequest } from '../../../../core/models';

@Component({
  selector: 'app-category-editor',
  imports: [
    ButtonModule,
    DialogModule,
    InputNumberModule,
    InputTextModule,
    ReactiveFormsModule,
    TextareaModule,
  ],
  templateUrl: './category-editor.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CategoryEditorComponent {
  // inputs
  visible = input<boolean>(false);
  category = input<Category | null>(null);

  // outputs
  saved = output<CreateCategoryRequest | UpdateCategoryRequest>();
  closed = output<void>();

  // state
  protected readonly isEditMode = computed(() => this.category() !== null);
  protected readonly dialogTitle = computed(() =>
    this.isEditMode() ? 'Edit Category' : 'New Category'
  );

  protected readonly categoryForm: FormGroup;
  protected readonly isSubmitting = signal(false);

  // services
  private readonly fb = inject(FormBuilder);

  constructor() {
    this.categoryForm = this.fb.group({
      name: [
        { value: '', disabled: false },
        [Validators.required, Validators.minLength(1), Validators.maxLength(100)],
      ],
      description: [null, [Validators.maxLength(500)]],
      sortOrder: [0, [Validators.min(0)]],
    });

    // Sync form when category changes
    effect(() => {
      const category = this.category();
      if (category) {
        this.categoryForm.patchValue({
          name: category.name,
          description: category.description,
          sortOrder: category.sortOrder,
        });
      } else {
        this.categoryForm.reset({
          name: '',
          description: null,
          sortOrder: 0,
        });
      }
    });
  }

  protected onSave(): void {
    if (this.categoryForm.invalid) {
      this.categoryForm.markAllAsTouched();
      return;
    }

    const formValue = this.categoryForm.getRawValue();

    if (this.isEditMode()) {
      const updateRequest: UpdateCategoryRequest = {
        name: formValue.name.trim(),
        description: formValue.description,
        sortOrder: formValue.sortOrder,
      };
      this.saved.emit(updateRequest);
    } else {
      const createRequest: CreateCategoryRequest = {
        name: formValue.name.trim(),
        description: formValue.description,
        sortOrder: formValue.sortOrder,
      };
      this.saved.emit(createRequest);
    }
  }

  protected onCancel(): void {
    this.closed.emit();
  }

  protected onHide(): void {
    this.closed.emit();
  }

  protected isFieldInvalid(fieldName: string): boolean {
    const field = this.categoryForm.get(fieldName);
    return !!(field?.invalid && (field?.dirty || field?.touched));
  }
}
