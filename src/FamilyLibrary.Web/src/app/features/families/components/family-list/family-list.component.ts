import { ChangeDetectionStrategy, Component } from '@angular/core';
import { ButtonModule } from 'primeng/button';

@Component({
  selector: 'app-family-list',
  imports: [ButtonModule],
  template: `
    <div class="space-y-6">
      <div class="flex items-center justify-between">
        <div>
          <h1 class="text-2xl font-bold text-gray-900">Families</h1>
          <p class="text-sm text-gray-500 mt-1">Manage loadable families and versions</p>
        </div>
        <p-button label="Upload Family" icon="pi pi-upload" severity="primary" />
      </div>
      
      <div class="bg-white rounded-lg shadow-sm border border-gray-200 p-8 text-center text-gray-500">
        <i class="pi pi-box text-4xl text-gray-300 mb-4"></i>
        <p>No families uploaded yet</p>
      </div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FamilyListComponent {}
