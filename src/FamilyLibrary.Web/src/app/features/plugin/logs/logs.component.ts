import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-logs',
  template: `
    <div class="space-y-2">
      <h1 class="text-xl font-semibold text-gray-900">Logs</h1>
      <p class="text-sm text-gray-500">Plugin logs viewer — coming soon.</p>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LogsComponent {}
