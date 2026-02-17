import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { ButtonModule } from 'primeng/button';

@Component({
  selector: 'app-empty-state',
  imports: [ButtonModule],
  template: `
    <div class="flex flex-col items-center justify-center py-12 px-4 text-center">
      <div class="w-16 h-16 rounded-full bg-gray-100 flex items-center justify-center mb-4">
        <i [class]="icon()" class="text-3xl text-gray-400"></i>
      </div>
      <h3 class="text-lg font-semibold text-gray-900 mb-2">{{ title() }}</h3>
      @if (description()) {
        <p class="text-gray-500 text-sm max-w-md mb-6">{{ description() }}</p>
      }
      @if (actionLabel()) {
        <p-button
          [label]="actionLabel()!"
          [icon]="actionIcon()"
          (onClick)="onActionClick()"
          severity="primary"
        />
      }
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EmptyStateComponent {
  readonly icon = input<string>('pi pi-inbox');
  readonly title = input.required<string>();
  readonly description = input<string>();
  readonly actionLabel = input<string>();
  readonly actionIcon = input<string>('pi pi-plus');

  readonly actionClicked = () => {};

  protected onActionClick(): void {
    this.actionClicked();
  }
}
