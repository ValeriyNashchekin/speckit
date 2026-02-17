import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';

@Component({
  selector: 'app-role-list',
  imports: [ButtonModule, TableModule, TagModule],
  template: `
    <div class="space-y-6">
      <div class="flex items-center justify-between">
        <div>
          <h1 class="text-2xl font-bold text-gray-900">Family Roles</h1>
          <p class="text-sm text-gray-500 mt-1">Manage functional roles for families</p>
        </div>
        <p-button label="New Role" icon="pi pi-plus" severity="primary" />
      </div>
      
      <div class="bg-white rounded-lg shadow-sm border border-gray-200">
        <p-table [value]="roles()" tableStyleClass="w-full">
          <ng-template #header>
            <tr>
              <th class="px-4 py-3 text-left text-sm font-semibold text-gray-700">Name</th>
              <th class="px-4 py-3 text-left text-sm font-semibold text-gray-700">Type</th>
              <th class="px-4 py-3 text-left text-sm font-semibold text-gray-700">Category</th>
              <th class="px-4 py-3 text-left text-sm font-semibold text-gray-700">Description</th>
              <th class="px-4 py-3 text-right text-sm font-semibold text-gray-700">Actions</th>
            </tr>
          </ng-template>
          <ng-template #body let-role>
            <tr class="border-t border-gray-100 hover:bg-gray-50">
              <td class="px-4 py-3">
                <span class="font-medium text-gray-900">{{ role.name }}</span>
              </td>
              <td class="px-4 py-3">
                <p-tag 
                  [value]="role.type" 
                  [severity]="role.type === 'Loadable' ? 'info' : 'warn'"
                />
              </td>
              <td class="px-4 py-3 text-gray-600">{{ role.category ?? '-' }}</td>
              <td class="px-4 py-3 text-gray-600">{{ role.description ?? '-' }}</td>
              <td class="px-4 py-3 text-right">
                <div class="flex items-center justify-end gap-2">
                  <p-button icon="pi pi-pencil" [rounded]="true" [text]="true" severity="secondary" />
                  <p-button icon="pi pi-trash" [rounded]="true" [text]="true" severity="danger" />
                </div>
              </td>
            </tr>
          </ng-template>
          <ng-template #emptymessage>
            <tr>
              <td colspan="5" class="px-4 py-8 text-center text-gray-500">
                <div class="flex flex-col items-center gap-2">
                  <i class="pi pi-tags text-3xl text-gray-300"></i>
                  <span>No family roles found</span>
                  <p-button label="Create First Role" icon="pi pi-plus" size="small" />
                </div>
              </td>
            </tr>
          </ng-template>
        </p-table>
      </div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RoleListComponent {
  protected readonly roles = signal<Array<{
    id: string;
    name: string;
    type: 'Loadable' | 'System';
    category: string | null;
    description: string | null;
  }>>([]);
}
