import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { TableModule, TableLazyLoadEvent } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { QueueService } from '../../services/queue.service';
import { SystemType, SystemFamilyGroup } from '../../../../core/models/system-type.model';

@Component({
  selector: 'app-system-type-list',
  imports: [
    TableModule,
    TagModule,
    ButtonModule,
    InputTextModule,
    SelectModule,
    FormsModule,
  ],
  templateUrl: './system-type-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SystemTypeListComponent {
  private readonly queueService = inject(QueueService);

  protected readonly systemTypes = signal<SystemType[]>([]);
  protected readonly loading = signal(true);
  protected readonly totalRecords = signal(0);
  protected readonly searchTerm = signal('');
  protected readonly selectedGroup = signal<SystemFamilyGroup | null>(null);
  protected readonly rowsPerPage = signal(20);
  protected readonly first = signal(0);

  protected readonly groupOptions: { label: string; value: SystemFamilyGroup }[] = [
    { label: 'Group A', value: 'GroupA' },
    { label: 'Group B', value: 'GroupB' },
    { label: 'Group C', value: 'GroupC' },
    { label: 'Group D', value: 'GroupD' },
    { label: 'Group E', value: 'GroupE' },
  ];

  constructor() {
    this.loadSystemTypes();
  }

  protected loadSystemTypes(): void {
    this.loading.set(true);
    const page = Math.floor(this.first() / this.rowsPerPage()) + 1;

    this.queueService
      .getSystemTypes(page, this.rowsPerPage(), {
        searchTerm: this.searchTerm() || undefined,
        category: this.selectedGroup(),
      })
      .subscribe({
        next: result => {
          this.systemTypes.set(result.data);
          this.totalRecords.set(result.totalCount);
          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
        },
      });
  }

  protected onLazyLoad(event: TableLazyLoadEvent): void {
    this.first.set(event.first ?? 0);
    this.rowsPerPage.set(event.rows ?? 20);
    this.loadSystemTypes();
  }

  protected onSearch(): void {
    this.first.set(0);
    this.loadSystemTypes();
  }

  protected onFilterChange(): void {
    this.first.set(0);
    this.loadSystemTypes();
  }

  protected clearFilters(): void {
    this.searchTerm.set('');
    this.selectedGroup.set(null);
    this.first.set(0);
    this.loadSystemTypes();
  }

  protected getGroupLabel(group: SystemFamilyGroup): string {
    const labels: Record<SystemFamilyGroup, string> = {
      GroupA: 'Group A',
      GroupB: 'Group B',
      GroupC: 'Group C',
      GroupD: 'Group D',
      GroupE: 'Group E',
    };
    return labels[group] ?? group;
  }

  protected getGroupSeverity(group: SystemFamilyGroup): 'success' | 'info' | 'warn' | 'danger' | 'secondary' | 'contrast' {
    const severities: Record<SystemFamilyGroup, 'success' | 'info' | 'warn' | 'danger' | 'secondary' | 'contrast'> = {
      GroupA: 'success',
      GroupB: 'info',
      GroupC: 'warn',
      GroupD: 'danger',
      GroupE: 'secondary',
    };
    return severities[group] ?? 'secondary';
  }
}
