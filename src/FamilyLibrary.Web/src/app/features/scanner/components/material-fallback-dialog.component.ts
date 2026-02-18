import { ChangeDetectionStrategy, Component, computed, effect, inject, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CheckboxModule } from 'primeng/checkbox';
import { DialogModule } from 'primeng/dialog';
import { RadioButtonModule } from 'primeng/radiobutton';
import { TableModule } from 'primeng/table';
import { MaterialFallbackEvent, MaterialOption } from '../../../core/models/webview-events.model';
import { RevitBridgeService } from '../../../core/services/revit-bridge.service';

/**
 * Selection result when user chooses a material option.
 */
export interface MaterialFallbackResult {
  selectedOption: MaterialOption;
  rememberMapping: boolean;
  projectMaterialName: string;
}

/**
 * Dialog shown when a material is not found during Pull Update.
 * Allows user to select an existing material, create new, use default, or skip.
 */
@Component({
  selector: 'app-material-fallback-dialog',
  imports: [
    ButtonModule,
    CheckboxModule,
    DialogModule,
    FormsModule,
    RadioButtonModule,
    TableModule,
  ],
  templateUrl: './material-fallback-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MaterialFallbackDialogComponent {
  // inputs
  visible = input<boolean>(false);
  fallbackEvent = input<MaterialFallbackEvent['payload'] | null>(null);
  projectId = input<string>('');

  // outputs
  resolved = output<MaterialFallbackResult>();
  closed = output<void>();

  // services
  private readonly revitBridge = inject(RevitBridgeService);

  // state
  protected readonly selectedOptionId = signal<string>('');
  protected readonly rememberMapping = signal(false);

  // Computed: available options grouped by type
  protected readonly existingMaterials = computed(() =>
    (this.fallbackEvent()?.availableOptions ?? []).filter((o) => o.type === 'existing')
  );

  protected readonly specialOptions = computed(() =>
    (this.fallbackEvent()?.availableOptions ?? []).filter((o) => o.type !== 'existing')
  );

  protected readonly selectedOption = computed(() =>
    (this.fallbackEvent()?.availableOptions ?? []).find((o) => o.id === this.selectedOptionId())
  );

  protected readonly canProceed = computed(() => this.selectedOptionId() !== '');

  protected readonly missingMaterialName = computed(
    () => this.fallbackEvent()?.missingMaterial.templateMaterialName ?? ''
  );

  protected readonly systemTypeName = computed(
    () => this.fallbackEvent()?.systemTypeName ?? ''
  );

  // Initialize selection when event changes
  constructor() {
    effect(() => {
      const event = this.fallbackEvent();
      if (event && event.availableOptions.length > 0) {
        // Default to first existing material, or 'default' option
        const existingOption = event.availableOptions.find((o) => o.type === 'existing');
        const defaultOption = event.availableOptions.find((o) => o.type === 'default');
        this.selectedOptionId.set(existingOption?.id ?? defaultOption?.id ?? '');
      }
    });
  }

  protected onOptionChange(value: string): void {
    this.selectedOptionId.set(value);
  }

  protected onRememberChange(checked: boolean): void {
    this.rememberMapping.set(checked);
  }

  protected onProceed(): void {
    const option = this.selectedOption();
    if (!option) return;

    const result: MaterialFallbackResult = {
      selectedOption: option,
      rememberMapping: this.rememberMapping(),
      projectMaterialName: option.name,
    };

    // If user wants to remember the mapping, send to plugin
    if (this.rememberMapping() && this.projectId()) {
      this.revitBridge.saveMaterialMapping({
        projectId: this.projectId(),
        templateMaterialName: this.missingMaterialName(),
        projectMaterialName: option.name,
        applyToCurrent: true,
      });
    }

    this.resolved.emit(result);
    this.reset();
  }

  protected onCancel(): void {
    this.closed.emit();
    this.reset();
  }

  protected onHide(): void {
    this.closed.emit();
    this.reset();
  }

  private reset(): void {
    this.selectedOptionId.set('');
    this.rememberMapping.set(false);
  }

  protected getOptionIcon(type: MaterialOption['type']): string {
    const icons: Record<MaterialOption['type'], string> = {
      existing: 'pi-box',
      create: 'pi-plus',
      default: 'pi-circle',
      skip: 'pi-forward',
    };
    return icons[type];
  }

  protected getOptionSeverity(type: MaterialOption['type']): 'success' | 'warn' | 'secondary' | 'info' {
    const severities: Record<MaterialOption['type'], 'success' | 'warn' | 'secondary' | 'info'> = {
      existing: 'success',
      create: 'info',
      default: 'warn',
      skip: 'secondary',
    };
    return severities[type];
  }
}
