import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { TableModule, TableLazyLoadEvent } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { Family } from '../../../../core/models/family.model';

@Component({
  selector: 'app-family-table',
  imports: [ButtonModule, DatePipe, TableModule, TagModule, TooltipModule],
  templateUrl: './family-table.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FamilyTableComponent {
  // Inputs
  families = input.required<Family[]>();
  loading = input<boolean>(false);
  totalRecords = input<number>(0);
  first = input<number>(0);

  // Outputs
  lazyLoad = output<TableLazyLoadEvent>();
  selectFamily = output<Family>();

  // Table configuration
  protected readonly rows = 20;
  protected readonly virtualScrollItemSize = 48;

  protected onLazyLoad(event: TableLazyLoadEvent): void {
    this.lazyLoad.emit(event);
  }

  protected onSelectFamily(family: Family): void {
    this.selectFamily.emit(family);
  }

  protected trackByFamilyId(index: number, family: Family): string {
    return family.id;
  }
}
