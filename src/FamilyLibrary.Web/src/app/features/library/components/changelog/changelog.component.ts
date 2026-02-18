import { ChangeDetectionStrategy, Component, computed, inject, input, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TimelineModule } from 'primeng/timeline';
import { TagModule } from 'primeng/tag';
import { SelectModule } from 'primeng/select';
import { PanelModule } from 'primeng/panel';
import { DividerModule } from 'primeng/divider';
import { SkeletonModule } from 'primeng/skeleton';
import { ButtonModule } from 'primeng/button';
import { LibraryService } from '../../services/library.service';
import { FamilyVersion } from '../../../../core/models/family.model';
import { ChangeItem, ChangeSet } from '../../../../core/models/scanner.models';
import { ChangeItemComponent } from './change-item.component';

/**
 * Version selection option for the dropdown.
 */
interface VersionOption {
  label: string;
  value: number;
}

/**
 * Internal interface for timeline events.
 */
interface TimelineEvent {
  version: number;
  date: string;
  message: string;
  author: string;
  changeSet?: ChangeSet;
}

/**
 * Component to display changelog with version selection and grouped changes.
 * Shows version history using PrimeNG Timeline with changes organized by category.
 */
@Component({
  selector: 'app-changelog',
  imports: [
    TimelineModule,
    TagModule,
    SelectModule,
    PanelModule,
    DividerModule,
    SkeletonModule,
    ButtonModule,
    FormsModule,
    DatePipe,
    ChangeItemComponent,
  ],
  templateUrl: './changelog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChangelogComponent {
  private readonly libraryService = inject(LibraryService);

  // Inputs
  familyId = input.required<string>();
  selectedVersions = input<number[]>([]);

  // State
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly versions = signal<FamilyVersion[]>([]);
  protected readonly selectedVersionFilter = signal<number | null>(null);

  // Computed properties
  protected readonly versionOptions = computed<VersionOption[]>(() => {
    const versionList = this.versions();
    return versionList.map(v => ({
      label: `Version ${v.version}`,
      value: v.version,
    }));
  });

  protected readonly timelineEvents = computed<TimelineEvent[]>(() => {
    const versionList = this.versions();
    const filter = this.selectedVersionFilter();
    const selected = this.selectedVersions();

    let filtered = versionList;

    if (filter !== null) {
      filtered = versionList.filter(v => v.version === filter);
    } else if (selected.length > 0) {
      filtered = versionList.filter(v => selected.includes(v.version));
    }

    return filtered
      .sort((a, b) => b.version - a.version)
      .map(v => ({
        version: v.version,
        date: v.publishedAt,
        message: v.commitMessage ?? 'No commit message',
        author: v.publishedBy,
        changeSet: this.parseChangeSet(v),
      }));
  });

  protected readonly hasChanges = computed(() => {
    return this.timelineEvents().some(e => e.changeSet?.hasChanges ?? false);
  });

  constructor() {
    this.loadVersions();
  }

  /**
   * Load version history from the API.
   */
  protected loadVersions(): void {
    const id = this.familyId();
    if (!id) {
      this.error.set('Family ID not provided');
      this.loading.set(false);
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    this.libraryService.getFamilyVersions(id).subscribe({
      next: data => {
        this.versions.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load changelog');
        this.loading.set(false);
      },
    });
  }

  /**
   * Handle version filter change.
   */
  protected onVersionChange(version: number | null): void {
    this.selectedVersionFilter.set(version);
  }

  /**
   * Clear version filter.
   */
  protected clearFilter(): void {
    this.selectedVersionFilter.set(null);
  }

  /**
   * Parse version snapshot into a ChangeSet.
   * The snapshot contains the state at that version.
   */
  private parseChangeSet(version: FamilyVersion): ChangeSet {
    // For now, create a basic change set from commit message
    // In a real implementation, this would compare with previous version
    const items: ChangeItem[] = [];

    if (version.commitMessage) {
      // Add a generic change item based on commit message
      items.push({
        category: 'Txt',
        previousValue: undefined,
        currentValue: version.commitMessage,
      });
    }

    return {
      items,
      hasChanges: items.length > 0,
    };
  }

  /**
   * Track timeline events by version.
   */
  protected trackByVersion(_index: number, event: TimelineEvent): number {
    return event.version;
  }
}
