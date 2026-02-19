import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-commands',
  template: `
    <div class="space-y-2">
      <h1 class="text-xl font-semibold text-gray-900">Commands</h1>
      <p class="text-sm text-gray-500">Plugin command management — coming soon.</p>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CommandsComponent {}
