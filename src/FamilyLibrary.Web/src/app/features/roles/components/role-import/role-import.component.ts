import { ChangeDetectionStrategy, Component, computed, inject, input, output, signal } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { FileUploadModule } from 'primeng/fileupload';
import { MessageModule } from 'primeng/message';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';

import { Category, ImportPreviewItem, ImportPreviewResponse, ImportResultResponse, RoleType } from '../../../../core/models';

interface FileSelectEvent {
  files: File[];
}

@Component({
  selector: 'app-role-import',
  imports: [ButtonModule, DialogModule, FileUploadModule, MessageModule, TableModule, TagModule],
  templateUrl: './role-import.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RoleImportComponent {
  // inputs
  visible = input<boolean>(false);
  categories = input<Array<Category>>([]);
  previewData = input<ImportPreviewResponse | null>(null);
  importResult = input<ImportResultResponse | null>(null);

  // outputs
  fileSelected = output<File>();
  importConfirmed = output<void>();
  closed = output<void>();

  // state
  protected readonly selectedFile = signal<File | null>(null);
  protected readonly isProcessing = signal(false);
  protected readonly isImporting = signal(false);
  protected readonly currentStep = signal<'upload' | 'preview' | 'result'>('upload');

  // computed
  protected readonly hasValidItems = computed(() => {
    const preview = this.previewData();
    return preview !== null && preview.totalValid > 0;
  });

  protected readonly hasPreviewData = computed(() => {
    return this.previewData() !== null;
  });

  protected readonly validItems = computed(() => {
    const preview = this.previewData();
    if (!preview) return [];
    return preview.items.filter(item => item.isValid && !item.isDuplicate);
  });

  protected readonly duplicateItems = computed(() => {
    const preview = this.previewData();
    if (!preview) return [];
    return preview.items.filter(item => item.isDuplicate);
  });

  protected readonly invalidItems = computed(() => {
    const preview = this.previewData();
    if (!preview) return [];
    return preview.items.filter(item => !item.isValid);
  });

  protected readonly categoryNameMap = computed(() => {
    const cats = this.categories();
    const map = new Map<string, string>();
    cats.forEach(c => map.set(c.id, c.name));
    return map;
  });

  // dialog state
  protected readonly dialogTitle = computed(() => {
    const step = this.currentStep();
    switch (step) {
      case 'upload': return 'Import Roles';
      case 'preview': return 'Preview Import';
      case 'result': return 'Import Results';
    }
  });

  protected onHide(): void {
    this.resetState();
    this.closed.emit();
  }

  protected onFileSelect(event: FileSelectEvent): void {
    const file = event.files[0];
    if (file) {
      this.selectedFile.set(file);
      this.fileSelected.emit(file);
      this.isProcessing.set(true);
    }
  }

  protected onUploadProcessComplete(): void {
    this.isProcessing.set(false);
    if (this.hasPreviewData()) {
      this.currentStep.set('preview');
    }
  }

  protected onConfirmImport(): void {
    this.isImporting.set(true);
    this.importConfirmed.emit();
  }

  protected onImportComplete(): void {
    this.isImporting.set(false);
    this.currentStep.set('result');
  }

  protected onCancel(): void {
    this.resetState();
    this.closed.emit();
  }

  protected onBack(): void {
    this.selectedFile.set(null);
    this.currentStep.set('upload');
  }

  protected onNewImport(): void {
    this.resetState();
    this.currentStep.set('upload');
  }

  protected getSeverity(type: RoleType): 'info' | 'warn' {
    return type === 'Loadable' ? 'info' : 'warn';
  }

  protected getCategoryName(categoryId: string | null): string {
    if (!categoryId) return '-';
    return this.categoryNameMap().get(categoryId) ?? '-';
  }

  private resetState(): void {
    this.selectedFile.set(null);
    this.isProcessing.set(false);
    this.isImporting.set(false);
    this.currentStep.set('upload');
  }
}
