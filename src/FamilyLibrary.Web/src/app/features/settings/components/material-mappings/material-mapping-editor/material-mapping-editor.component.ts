import { ChangeDetectionStrategy, Component, computed, effect, inject, input, output, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';

import { MaterialMapping, CreateMaterialMappingRequest, UpdateMaterialMappingRequest } from '../../../../../core/models';

/**
 * Dialog component for creating and editing material mappings.
 */
@Component({
  selector: 'app-material-mapping-editor',
  imports: [ButtonModule, DialogModule, InputTextModule, ReactiveFormsModule],
  templateUrl: './material-mapping-editor.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MaterialMappingEditorComponent {
  // inputs
  visible = input<boolean>(false);
  mapping = input<MaterialMapping | null>(null);
  projectId = input<string>('');

  // outputs
  saved = output<CreateMaterialMappingRequest | UpdateMaterialMappingRequest>();
  closed = output<void>();

  // state
  protected readonly isEditMode = computed(() => this.mapping() !== null);
  protected readonly dialogTitle = computed(() =>
    this.isEditMode() ? 'Edit Material Mapping' : 'New Material Mapping'
  );

  protected readonly mappingForm: FormGroup;
  protected readonly isSubmitting = signal(false);

  // services
  private readonly fb = inject(FormBuilder);

  constructor() {
    this.mappingForm = this.fb.group({
      templateMaterialName: ['', [Validators.required, Validators.minLength(1), Validators.maxLength(200)]],
      projectMaterialName: ['', [Validators.required, Validators.minLength(1), Validators.maxLength(200)]],
      category: ['', [Validators.maxLength(100)]],
    });

    // Sync form when mapping changes
    effect(() => {
      const mapping = this.mapping();
      if (mapping) {
        this.mappingForm.patchValue({
          templateMaterialName: mapping.templateMaterialName,
          projectMaterialName: mapping.projectMaterialName,
          category: mapping.category ?? '',
        });
      } else {
        this.mappingForm.reset({
          templateMaterialName: '',
          projectMaterialName: '',
          category: '',
        });
      }
    });
  }

  protected onSave(): void {
    if (this.mappingForm.invalid) {
      this.mappingForm.markAllAsTouched();
      return;
    }

    const formValue = this.mappingForm.getRawValue();

    if (this.isEditMode()) {
      const updateRequest: UpdateMaterialMappingRequest = {
        projectMaterialName: formValue.projectMaterialName.trim(),
        category: formValue.category?.trim() || undefined,
      };
      this.saved.emit(updateRequest);
    } else {
      const createRequest: CreateMaterialMappingRequest = {
        projectId: this.projectId() || 'default',
        templateMaterialName: formValue.templateMaterialName.trim(),
        projectMaterialName: formValue.projectMaterialName.trim(),
        category: formValue.category?.trim() || undefined,
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
    const field = this.mappingForm.get(fieldName);
    return !!(field?.invalid && (field?.dirty || field?.touched));
  }
}
