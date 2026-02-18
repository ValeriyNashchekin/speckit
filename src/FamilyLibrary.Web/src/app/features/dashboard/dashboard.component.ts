import { ChangeDetectionStrategy, Component, signal } from '@angular/core';

@Component({
  selector: 'app-dashboard',
  template: `
    <div class="space-y-6">
      <h1 class="text-2xl font-bold text-gray-900">Dashboard</h1>
      <p class="text-gray-600">Welcome to Family Library MVP. Select an option from the sidebar to get started.</p>
      
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <div class="bg-white p-6 rounded-lg shadow-sm border border-gray-200">
          <div class="flex items-center gap-3">
            <div class="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center">
              <i class="pi pi-tags text-blue-600"></i>
            </div>
            <div>
              <p class="text-sm text-gray-500">Family Roles</p>
              <p class="text-xl font-semibold text-gray-900">{{ stats().roles }}</p>
            </div>
          </div>
        </div>
        
        <div class="bg-white p-6 rounded-lg shadow-sm border border-gray-200">
          <div class="flex items-center gap-3">
            <div class="w-10 h-10 bg-green-100 rounded-lg flex items-center justify-center">
              <i class="pi pi-box text-green-600"></i>
            </div>
            <div>
              <p class="text-sm text-gray-500">Families</p>
              <p class="text-xl font-semibold text-gray-900">{{ stats().families }}</p>
            </div>
          </div>
        </div>
        
        <div class="bg-white p-6 rounded-lg shadow-sm border border-gray-200">
          <div class="flex items-center gap-3">
            <div class="w-10 h-10 bg-yellow-100 rounded-lg flex items-center justify-center">
              <i class="pi pi-file-edit text-yellow-600"></i>
            </div>
            <div>
              <p class="text-sm text-gray-500">Drafts</p>
              <p class="text-xl font-semibold text-gray-900">{{ stats().drafts }}</p>
            </div>
          </div>
        </div>
        
        <div class="bg-white p-6 rounded-lg shadow-sm border border-gray-200">
          <div class="flex items-center gap-3">
            <div class="w-10 h-10 bg-purple-100 rounded-lg flex items-center justify-center">
              <i class="pi pi-folder text-purple-600"></i>
            </div>
            <div>
              <p class="text-sm text-gray-500">Categories</p>
              <p class="text-xl font-semibold text-gray-900">{{ stats().categories }}</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardComponent {
  protected readonly stats = signal({
    roles: 0,
    families: 0,
    drafts: 0,
    categories: 0,
  });
}
