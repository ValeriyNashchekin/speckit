import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ParameterChange } from '../../../../core/models/scanner.models';

/**
 * Type of parameter change
 */
export type ParameterChangeType = 'added' | 'removed' | 'changed';

/**
 * Internal representation of parameter change with change type
 */
interface ParameterChangeRow extends ParameterChange {
  changeType: ParameterChangeType;
}

/**
 * Component to display parameter changes in a diff-style format.
 * Highlights added, removed and changed parameters.
 */
@Component({
  selector: 'app-parameter-diff',
  imports: [TableModule, TagModule],
  templateUrl: './parameter-diff.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ParameterDiffComponent {
  /**
   * Array of parameter changes to display
   */
  changes = input.required<ParameterChange[]>();

  /**
   * Processed changes with computed change types
   */
  protected readonly processedChanges = computed<ParameterChangeRow[]>(() => {
    const changes = this.changes();
    if (!changes?.length) {
      return [];
    }

    return changes.map((change) => ({
      ...change,
      changeType: this.getChangeType(change),
    }));
  });

  /**
   * Check if there are any changes to display
   */
  protected readonly hasChanges = computed(() => this.processedChanges().length > 0);

  /**
   * Determine the type of change for a parameter
   */
  private getChangeType(change: ParameterChange): ParameterChangeType {
    const hasPrevious = change.previousValue !== undefined && change.previousValue !== null;
    const hasCurrent = change.currentValue !== undefined && change.currentValue !== null;

    if (!hasPrevious && hasCurrent) {
      return 'added';
    }
    if (hasPrevious && !hasCurrent) {
      return 'removed';
    }
    return 'changed';
  }

  /**
   * Track changes by parameter name
   */
  protected trackByParameterName(_index: number, change: ParameterChangeRow): string {
    return change.name;
  }
}
