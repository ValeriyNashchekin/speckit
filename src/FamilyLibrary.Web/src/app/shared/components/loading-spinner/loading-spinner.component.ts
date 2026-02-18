import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

@Component({
  selector: 'app-loading-spinner',
  imports: [ProgressSpinnerModule],
  template: `
    <div class="flex flex-col items-center justify-center gap-4 p-8" [class.h-full]="fullHeight()">
      <p-progressspinner
        [styleClass]="size() === 'small' ? 'w-8 h-8' : size() === 'large' ? 'w-16 h-16' : 'w-12 h-12'"
        strokeWidth="3"
        fill="transparent"
      />
      @if (message()) {
        <span class="text-gray-500 text-sm">{{ message() }}</span>
      }
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoadingSpinnerComponent {
  readonly message = input<string>('');
  readonly size = input<'small' | 'medium' | 'large'>('medium');
  readonly fullHeight = input<boolean>(false);
}
