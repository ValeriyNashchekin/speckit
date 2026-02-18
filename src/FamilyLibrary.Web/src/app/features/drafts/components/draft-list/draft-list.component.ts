import { ChangeDetectionStrategy, Component } from '@angular/core';
import { ButtonModule } from 'primeng/button';

@Component({
  selector: 'app-draft-list',
  imports: [ButtonModule],
  template: `
    <div class="space-y-6">
      <div class="flex items-center justify-between">
        <div>
          <h1 class="text-2xl font-bold text-gray-900">Drafts</h1>
          <p class="text-sm text-gray-500 mt-1">Families being prepared for publish</p>
        </div>
      </div>
      
      <div class="bg-white rounded-lg shadow-sm border border-gray-200 p-8 text-center text-gray-500">
        <i class="pi pi-file-edit text-4xl text-gray-300 mb-4"></i>
        <p>No drafts available</p>
      </div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DraftListComponent {}
