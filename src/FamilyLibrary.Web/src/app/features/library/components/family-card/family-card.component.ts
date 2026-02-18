import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { Family } from '../../../../core/models/family.model';

@Component({
  selector: 'app-family-card',
  imports: [ButtonModule, CardModule, DatePipe, TagModule],
  templateUrl: './family-card.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FamilyCardComponent {
  family = input.required<Family>();
  select = output<Family>();

  protected getInitials(name: string): string {
    return name
      .split(' ')
      .map(word => word.charAt(0))
      .join('')
      .substring(0, 2)
      .toUpperCase();
  }

  protected onSelect(): void {
    this.select.emit(this.family());
  }
}
