import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { Family } from '../../../../core/models/family.model';

@Component({
  selector: 'app-family-card',
  imports: [DatePipe],
  templateUrl: './family-card.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FamilyCardComponent {
  family = input.required<Family>();
  select = output<Family>();

  protected onSelect(): void {
    this.select.emit(this.family());
  }
}
