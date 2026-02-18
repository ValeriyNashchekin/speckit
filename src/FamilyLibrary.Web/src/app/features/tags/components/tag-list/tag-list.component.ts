import { ChangeDetectionStrategy, Component } from '@angular/core';
import { ButtonModule } from 'primeng/button';

@Component({
  selector: 'app-tag-list',
  imports: [ButtonModule],
  template: `
    <div class="space-y-6">
      <div class="flex items-center justify-between">
        <div>
          <h1 class="text-2xl font-bold text-gray-900">Tags</h1>
          <p class="text-sm text-gray-500 mt-1">Manage tags for filtering and categorization</p>
        </div>
        <p-button label="New Tag" icon="pi pi-plus" severity="primary" />
      </div>
      
      <div class="bg-white rounded-lg shadow-sm border border-gray-200 p-8 text-center text-gray-500">
        <i class="pi pi-label text-4xl text-gray-300 mb-4"></i>
        <p>No tags yet</p>
      </div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TagListComponent {}
