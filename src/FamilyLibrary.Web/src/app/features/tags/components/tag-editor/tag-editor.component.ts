import { ChangeDetectionStrategy, Component, computed, effect, inject, input, output, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { ColorPickerChangeEvent, ColorPickerModule } from 'primeng/colorpicker';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';

import { Tag, CreateTagRequest, UpdateTagRequest } from '../../../../core/models';

const DEFAULT_COLOR = '3B82F6';

@Component({
  selector: 'app-tag-editor',
  imports: [ButtonModule, ColorPickerModule, DialogModule, InputTextModule, ReactiveFormsModule],
  templateUrl: './tag-editor.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TagEditorComponent {
  // inputs
  visible = input<boolean>(false);
  tag = input<Tag | null>(null);

  // outputs
  saved = output<CreateTagRequest | UpdateTagRequest>();
  closed = output<void>();

  // state
  protected readonly isEditMode = computed(() => this.tag() !== null);
  protected readonly dialogTitle = computed(() => (this.isEditMode() ? 'Edit Tag' : 'New Tag'));

  protected readonly tagForm: FormGroup;
  protected readonly isSubmitting = signal(false);
  protected readonly colorValue = signal<string>(DEFAULT_COLOR);

  // services
  private readonly fb = inject(FormBuilder);

  constructor() {
    this.tagForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(1), Validators.maxLength(50)]],
      color: [DEFAULT_COLOR],
    });

    // Sync form when tag changes
    effect(() => {
      const tag = this.tag();
      if (tag) {
        this.tagForm.patchValue({
          name: tag.name,
          color: tag.color?.replace('#', '') ?? DEFAULT_COLOR,
        });
        this.colorValue.set(tag.color?.replace('#', '') ?? DEFAULT_COLOR);
      } else {
        this.tagForm.reset({
          name: '',
          color: DEFAULT_COLOR,
        });
        this.colorValue.set(DEFAULT_COLOR);
      }
    });
  }

  protected onColorChange(event: ColorPickerChangeEvent): void {
    if (event.value) {
      this.colorValue.set(event.value as string);
    }
  }

  protected onSave(): void {
    if (this.tagForm.invalid) {
      this.tagForm.markAllAsTouched();
      return;
    }

    const formValue = this.tagForm.getRawValue();
    const color = '#' + (formValue.color || this.colorValue());

    if (this.isEditMode()) {
      const updateRequest: UpdateTagRequest = {
        name: formValue.name.trim(),
        color: color,
      };
      this.saved.emit(updateRequest);
    } else {
      const createRequest: CreateTagRequest = {
        name: formValue.name.trim(),
        color: color,
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
    const field = this.tagForm.get(fieldName);
    return !!(field?.invalid && (field?.dirty || field?.touched));
  }
}
