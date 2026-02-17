import { ChangeDetectionStrategy, Component, computed, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';

export interface MissingMaterial {
  name: string;
  layerFunction: string;
  action: 'create' | 'skip';
}

interface ActionOption {
  label: string;
  value: 'create' | 'skip';
}

@Component({
  selector: 'app-material-warning-dialog',
  imports: [DialogModule, TableModule, SelectModule, ButtonModule, FormsModule],
  templateUrl: './material-warning-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MaterialWarningDialogComponent {
  // Inputs
  materials = input<MissingMaterial[]>([]);
  visible = input<boolean>(false);

  // Outputs
  proceed = output<MissingMaterial[]>();
  cancel = output<void>();
  visibleChange = output<boolean>();

  // Internal state
  protected readonly internalMaterials = signal<MissingMaterial[]>([]);

  // Action options for dropdown
  protected readonly actionOptions: ActionOption[] = [
    { label: 'Create New Material', value: 'create' },
    { label: 'Skip', value: 'skip' },
  ];

  // Computed: check if any material has 'create' action
  protected readonly hasCreateAction = computed(() => 
    this.internalMaterials().some(m => m.action === 'create')
  );

  // Computed: count of materials to create
  protected readonly createCount = computed(() => 
    this.internalMaterials().filter(m => m.action === 'create').length
  );

  // Computed: count of materials to skip
  protected readonly skipCount = computed(() => 
    this.internalMaterials().filter(m => m.action === 'skip').length
  );

  // Initialize internal materials when materials input changes
  protected initializeMaterials(): void {
    const mats = this.materials();
    if (mats.length > 0 && this.internalMaterials().length === 0) {
      this.internalMaterials.set(mats.map(m => ({ ...m })));
    }
  }

  // Handle action change for a material
  protected onActionChange(material: MissingMaterial, action: 'create' | 'skip'): void {
    this.internalMaterials.update(materials =>
      materials.map(m => 
        m.name === material.name && m.layerFunction === material.layerFunction
          ? { ...m, action }
          : m
      )
    );
  }

  // Set all materials to specific action
  protected setAllActions(action: 'create' | 'skip'): void {
    this.internalMaterials.update(materials =>
      materials.map(m => ({ ...m, action }))
    );
  }

  // Handle proceed click
  protected onProceed(): void {
    this.proceed.emit(this.internalMaterials());
    this.reset();
  }

  // Handle cancel click
  protected onCancel(): void {
    this.cancel.emit();
    this.reset();
  }

  // Handle dialog hide
  protected onHide(): void {
    this.visibleChange.emit(false);
  }

  // Handle dialog show
  protected onShow(): void {
    this.initializeMaterials();
  }

  // Reset internal state
  private reset(): void {
    this.internalMaterials.set([]);
    this.visibleChange.emit(false);
  }
}
