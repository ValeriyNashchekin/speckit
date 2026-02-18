import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';

/**
 * TypeScript interfaces matching backend RoutingPreferencesDto.
 * Used for MEP System Types (Pipes, Ducts) - GroupB.
 */
export interface RoutingPreferencesJson {
  segments: Segment[];
  fittings: Fitting[];
}

export interface Segment {
  materialName: string;
  scheduleType: string;
}

export interface Fitting {
  familyName: string;
  typeName: string;
  angleRange: string | null;
}

@Component({
  selector: 'app-routing-preferences-display',
  imports: [CardModule, TableModule],
  templateUrl: './routing-preferences-display.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RoutingPreferencesDisplayComponent {
  /**
   * Input signal for the routing preferences to display.
   */
  preferences = input.required<RoutingPreferencesJson>();

  /**
   * Computed signal for segments count.
   */
  protected readonly segmentsCount = computed(() => this.preferences().segments.length);

  /**
   * Computed signal for fittings count.
   */
  protected readonly fittingsCount = computed(() => this.preferences().fittings.length);

  /**
   * Track segments by material name and schedule type.
   */
  protected trackBySegment(index: number, segment: Segment): string {
    return `${segment.materialName}-${segment.scheduleType}`;
  }

  /**
   * Track fittings by family name, type name, and angle range.
   */
  protected trackByFitting(index: number, fitting: Fitting): string {
    return `${fitting.familyName}-${fitting.typeName}-${fitting.angleRange ?? 'no-angle'}`;
  }
}
