import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { JsonPipe } from '@angular/common';
import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { SystemType } from '../../../../core/models/system-type.model';
import {
  RoutingPreferencesDisplayComponent,
  RoutingPreferencesJson,
} from '../../../library/components/system-type-detail/routing-preferences-display.component';

// Interfaces for parsed JSON structures
interface CompoundStructureLayer {
  function: string;
  thickness: number;
  material: string;
  isStructural: boolean;
  priority: number;
}

interface CompoundStructure {
  layers: CompoundStructureLayer[];
  totalThickness: number;
}

interface SimpleParameter {
  name: string;
  value: string | number | boolean;
  storageType: string;
}

type TagSeverity = 'secondary' | 'info' | 'warn' | 'success' | 'danger' | 'contrast';

@Component({
  selector: 'app-system-type-detail',
  imports: [CardModule, TableModule, TagModule, JsonPipe, RoutingPreferencesDisplayComponent],
  templateUrl: './system-type-detail.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SystemTypeDetailComponent {
  // Input signal for the system type to display
  systemType = input.required<SystemType>();

  // Computed signal for parsed JSON data
  protected readonly parsedJson = computed(() => {
    const rawJson = this.systemType().json;
    if (!rawJson) {
      return null;
    }

    try {
      return JSON.parse(rawJson);
    } catch {
      return null;
    }
  });

  // Computed signal for CompoundStructure (GroupA)
  protected readonly compoundStructure = computed((): CompoundStructure | null => {
    const data = this.parsedJson();
    if (!data || this.systemType().group !== 'GroupA') {
      return null;
    }

    // Handle both direct layers array and nested structure
    const layers = Array.isArray(data)
      ? data
      : data.layers ?? [];

    const parsedLayers: CompoundStructureLayer[] = layers.map((layer: Record<string, unknown>) => ({
      function: String(layer['function'] ?? layer['Function'] ?? ''),
      thickness: Number(layer['thickness'] ?? layer['Thickness'] ?? 0),
      material: String(layer['material'] ?? layer['Material'] ?? ''),
      isStructural: Boolean(layer['isStructural'] ?? layer['IsStructural'] ?? false),
      priority: Number(layer['priority'] ?? layer['Priority'] ?? 0),
    }));

    const totalThickness = parsedLayers.reduce((sum, layer) => sum + layer.thickness, 0);

    return {
      layers: parsedLayers,
      totalThickness,
    };
  });

  // Helper to convert unknown value to SimpleParameter value
  private toParameterValue(value: unknown): string | number | boolean {
    if (typeof value === 'string' || typeof value === 'number' || typeof value === 'boolean') {
      return value;
    }
    return String(value);
  }

  // Computed signal for Simple Parameters (GroupE)
  protected readonly parameters = computed((): SimpleParameter[] => {
    const data = this.parsedJson();
    if (!data || this.systemType().group !== 'GroupE') {
      return [];
    }

    // Handle both array and object formats
    if (Array.isArray(data)) {
      return data.map((param: Record<string, unknown>) => ({
        name: String(param['name'] ?? param['Name'] ?? param['key'] ?? ''),
        value: this.toParameterValue(param['value'] ?? param['Value'] ?? ''),
        storageType: String(param['storageType'] ?? param['StorageType'] ?? 'String'),
      }));
    }

    // Convert object key-value pairs to array
    return Object.entries(data).map(([key, value]) => ({
      name: key,
      value: this.toParameterValue(value),
      storageType: typeof value === 'number' ? 'Double' : typeof value === 'boolean' ? 'Integer' : 'String',
    }));
  });

  // Check if this is a GroupA (CompoundStructure) type
  protected readonly isCompoundStructure = computed(() => {
    return this.systemType().group === 'GroupA';
  });

  // Check if this is a GroupB (Routing Preferences) type
  protected readonly isRoutingPreferences = computed(() => {
    return this.systemType().group === 'GroupB';
  });

  // Check if this is a GroupE (Simple Parameters) type
  protected readonly isParameters = computed(() => {
    return this.systemType().group === 'GroupE';
  });

  // Computed signal for Routing Preferences (GroupB)
  protected readonly routingPreferences = computed((): RoutingPreferencesJson | null => {
    const data = this.parsedJson();
    if (!data || this.systemType().group !== 'GroupB') {
      return null;
    }

    // Parse segments array
    const segments = Array.isArray(data['segments'])
      ? data['segments'].map((seg: Record<string, unknown>) => ({
          materialName: String(seg['materialName'] ?? seg['MaterialName'] ?? ''),
          scheduleType: String(seg['scheduleType'] ?? seg['ScheduleType'] ?? ''),
        }))
      : [];

    // Parse fittings array
    const fittings = Array.isArray(data['fittings'])
      ? data['fittings'].map((fit: Record<string, unknown>) => {
          const angleValue = fit['angleRange'] ?? fit['AngleRange'];
          return {
            familyName: String(fit['familyName'] ?? fit['FamilyName'] ?? ''),
            typeName: String(fit['typeName'] ?? fit['TypeName'] ?? ''),
            angleRange: angleValue != null ? String(angleValue) : null,
          };
        })
      : [];

    return { segments, fittings };
  });

  // Format thickness value
  protected formatThickness(value: number): string {
    return value.toFixed(3);
  }

  // Get severity for structural layer indicator
  protected getStructuralSeverity(isStructural: boolean): TagSeverity {
    return isStructural ? 'success' : 'secondary';
  }

  // Get storage type severity
  protected getStorageTypeSeverity(storageType: string): TagSeverity {
    const severities: Record<string, TagSeverity> = {
      'Double': 'info',
      'Integer': 'warn',
      'String': 'secondary',
      'Boolean': 'contrast',
      'ElementId': 'danger',
    };
    return severities[storageType] ?? 'secondary';
  }

  // Format boolean value for display
  protected formatBooleanValue(value: boolean): string {
    return value ? 'Yes' : 'No';
  }

  // Format value for display
  protected formatValue(value: string | number | boolean): string {
    if (typeof value === 'boolean') {
      return this.formatBooleanValue(value);
    }
    return String(value);
  }
}
