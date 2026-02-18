import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { ButtonModule } from 'primeng/button';

@Component({
  selector: 'app-category-list',
  imports: [ButtonModule],
  template: `
    <div class="space-y-6">
      <div class="flex items-center justify-between">
        <div>
          <h1 class="text-2xl font-bold text-gray-900">Categories</h1>
          <p class="text-sm text-gray-500 mt-1">Organize roles into categories</p>
        </div>
        <p-button label="New Category" icon="pi pi-plus" severity="primary" />
      </div>
      
      <div class="bg-white rounded-lg shadow-sm border border-gray-200 p-8 text-center text-gray-500">
        <i class="pi pi-folder text-4xl text-gray-300 mb-4"></i>
        <p>No categories yet</p>
      </div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CategoryListComponent {}
