import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { output } from '@angular/core';

export interface PageHeaderAction {
  label: string;
  icon: string;
  severity?: 'primary' | 'secondary' | 'success' | 'info' | 'danger' | 'contrast';
  outlined?: boolean;
  disabled?: boolean;
}

@Component({
  selector: 'app-page-header',
  imports: [ButtonModule],
  template: `
    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 mb-6">
      <div>
        <h1 class="text-2xl font-bold text-gray-900">{{ title() }}</h1>
        @if (subtitle()) {
          <p class="text-sm text-gray-500 mt-1">{{ subtitle() }}</p>
        }
      </div>
      @if (actions().length > 0) {
        <div class="flex items-center gap-2 flex-wrap">
          @for (action of actions(); track action.label) {
            <p-button
              [label]="action.label"
              [icon]="action.icon"
              [severity]="action.severity ?? 'primary'"
              [outlined]="action.outlined ?? false"
              [disabled]="action.disabled ?? false"
              (onClick)="actionClick.emit(action)"
            />
          }
        </div>
      }
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PageHeaderComponent {
  readonly title = input.required<string>();
  readonly subtitle = input<string>();
  readonly actions = input<PageHeaderAction[]>([]);

  readonly actionClick = output<PageHeaderAction>();
}
