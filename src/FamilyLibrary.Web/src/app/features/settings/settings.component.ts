import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { inject } from '@angular/core';

@Component({
  selector: 'app-settings',
  imports: [FormsModule, InputTextModule, ButtonModule, SelectModule, ToastModule],
  template: `
    <div class="p-5 space-y-5">
      <div>
        <h1 class="text-lg font-semibold text-[var(--text-primary)]">Settings</h1>
        <p class="text-xs text-[var(--text-muted)] mt-0.5">Application configuration</p>
      </div>

      <div class="max-w-lg space-y-5">
        <div class="bg-white rounded-lg border border-[var(--border-subtle)] p-4 space-y-4">
          <h2 class="text-sm font-semibold text-[var(--text-primary)]">Connection</h2>
          <div class="space-y-1">
            <label class="text-xs text-[var(--text-muted)]" for="apiUrl">API URL</label>
            <input
              id="apiUrl"
              type="text"
              pInputText
              [(ngModel)]="apiUrl"
              class="w-full"
            />
          </div>
          <div class="space-y-1">
            <label class="text-xs text-[var(--text-muted)]" for="blobUrl">Blob Storage URL</label>
            <input
              id="blobUrl"
              type="text"
              pInputText
              [(ngModel)]="blobUrl"
              class="w-full"
            />
          </div>
        </div>

        <div class="bg-white rounded-lg border border-[var(--border-subtle)] p-4 space-y-4">
          <h2 class="text-sm font-semibold text-[var(--text-primary)]">Display</h2>
          <div class="space-y-1">
            <label class="text-xs text-[var(--text-muted)]" for="pageSize">Default page size</label>
            <p-select
              id="pageSize"
              [options]="pageSizeOptions"
              [(ngModel)]="defaultPageSize"
              optionLabel="label"
              optionValue="value"
              styleClass="w-full"
            />
          </div>
        </div>

        <div class="flex justify-end">
          <p-button label="Save" icon="pi pi-check" severity="primary" size="small" (onClick)="onSave()" />
        </div>
      </div>
    </div>
    <p-toast position="bottom-right" />
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SettingsComponent {
  private readonly messageService = inject(MessageService);

  protected apiUrl = signal('/api');
  protected blobUrl = signal('http://127.0.0.1:10000/devstoreaccount1');
  protected defaultPageSize = signal(20);

  protected readonly pageSizeOptions = [
    { label: '10', value: 10 },
    { label: '20', value: 20 },
    { label: '50', value: 50 },
    { label: '100', value: 100 },
  ];

  protected onSave(): void {
    this.messageService.add({
      severity: 'success',
      summary: 'Settings saved',
      detail: 'Configuration updated successfully',
    });
  }
}
