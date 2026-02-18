import { ChangeDetectionStrategy, Component, computed, effect, inject, input, output, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { MultiSelectModule } from 'primeng/multiselect';
import { SelectModule } from 'primeng/select';
import { TextareaModule } from 'primeng/textarea';

import { Category, CreateFamilyRoleRequest, FamilyRole, RoleType, Tag, UpdateFamilyRoleRequest } from '../../../../core/models';

@Component({
  selector: 'app-role-editor',
  imports: [ButtonModule, DialogModule, InputTextModule, MultiSelectModule, ReactiveFormsModule, SelectModule, TextareaModule],
  templateUrl: './role-editor.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RoleEditorComponent {
  // inputs
  visible = input<boolean>(false);
  role = input<FamilyRole | null>(null);
  categories = input<Array<Category>>([]);
  tags = input<Array<Tag>>([]);

  // outputs
  saved = output<CreateFamilyRoleRequest | UpdateFamilyRoleRequest>();
  closed = output<void>();

  // state
  protected readonly isEditMode = computed(() => this.role() !== null);
  protected readonly dialogTitle = computed(() => this.isEditMode() ? 'Edit Role' : 'New Role');

  protected readonly roleForm: FormGroup;
  protected readonly isSubmitting = signal(false);

  // dropdown options
  protected readonly typeOptions = signal<Array<{ label: string; value: RoleType }>>([
    { label: 'Loadable', value: 'Loadable' },
    { label: 'System', value: 'System' },
  ]);

  protected readonly categoryOptions = computed(() => {
    const cats = this.categories();
    return cats.map(c => ({ label: c.name, value: c.id }));
  });

  protected readonly tagOptions = computed(() => {
    const tagsList = this.tags();
    return tagsList.map(t => ({ label: t.name, value: t.id }));
  });

  // services
  private readonly fb = inject(FormBuilder);

  constructor() {
    this.roleForm = this.fb.group({
      name: [{ value: '', disabled: false }, [Validators.required, Validators.minLength(1), Validators.maxLength(100)]],
      type: ['Loadable', [Validators.required]],
      description: [null, [Validators.maxLength(500)]],
      categoryId: [null],
      tagIds: [[]],
    });

    // Sync form when role changes
    effect(() => {
      const role = this.role();
      if (role) {
        this.roleForm.patchValue({
          name: role.name,
          type: role.type,
          description: role.description,
          categoryId: role.categoryId,
          tagIds: role.tags?.map(t => t.id) ?? [],
        });
        this.roleForm.get('name')?.disable();
      } else {
        this.roleForm.reset({
          name: '',
          type: 'Loadable',
          description: null,
          categoryId: null,
          tagIds: [],
        });
        this.roleForm.get('name')?.enable();
      }
    });
  }

  protected onSave(): void {
    if (this.roleForm.invalid) {
      this.roleForm.markAllAsTouched();
      return;
    }

    const formValue = this.roleForm.getRawValue();

    if (this.isEditMode()) {
      const updateRequest: UpdateFamilyRoleRequest = {
        description: formValue.description,
        categoryId: formValue.categoryId,
        tagIds: formValue.tagIds,
      };
      this.saved.emit(updateRequest);
    } else {
      const createRequest: CreateFamilyRoleRequest = {
        name: formValue.name.trim(),
        type: formValue.type,
        description: formValue.description,
        categoryId: formValue.categoryId,
        tagIds: formValue.tagIds,
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
    const field = this.roleForm.get(fieldName);
    return !!(field?.invalid && (field?.dirty || field?.touched));
  }
}
