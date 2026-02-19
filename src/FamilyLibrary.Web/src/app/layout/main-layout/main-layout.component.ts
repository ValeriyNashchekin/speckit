import { ChangeDetectionStrategy, Component, computed, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { PanelMenuModule } from 'primeng/panelmenu';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { MenuItem } from 'primeng/api';

type UserRole = 'designer' | 'bim_manager' | 'admin';

@Component({
  selector: 'app-main-layout',
  imports: [
    RouterOutlet,
    PanelMenuModule,
    ConfirmDialogModule,
    ToastModule,
  ],
  templateUrl: './main-layout.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MainLayoutComponent {
  protected readonly userRole = signal<UserRole>('admin');

  protected readonly menuItems = computed<MenuItem[]>(() => {
    const role = this.userRole();
    const items: MenuItem[] = [];

    const libraryChildren: MenuItem[] = [
      { label: 'Browse', icon: 'pi pi-book', routerLink: '/library' },
    ];
    if (role === 'bim_manager' || role === 'admin') {
      libraryChildren.push({ label: 'Scanner', icon: 'pi pi-search', routerLink: '/scanner' });
    }
    items.push({ label: 'Library', expanded: true, items: libraryChildren });

    if (role === 'bim_manager' || role === 'admin') {
      items.push({
        label: 'Library Admin',
        expanded: true,
        items: [
          { label: 'Queue', icon: 'pi pi-list', routerLink: '/queue' },
          { label: 'Drafts', icon: 'pi pi-file-edit', routerLink: '/drafts' },
        ],
      });
    }

    if (role === 'admin') {
      items.push({
        label: 'Family Identification',
        expanded: true,
        items: [
          { label: 'Family Id', icon: 'pi pi-id-card', routerLink: '/roles' },
          { label: 'Catalog Categories', icon: 'pi pi-folder', routerLink: '/categories' },
          { label: 'Tags', icon: 'pi pi-tag', routerLink: '/tags' },
          { label: 'Recognition Rules', icon: 'pi pi-link', routerLink: '/recognition-rules' },
          { label: 'Settings', icon: 'pi pi-sliders-h', routerLink: '/settings' },
        ],
      });

      items.push({
        label: 'Plugin',
        expanded: true,
        items: [
          { label: 'Commands', icon: 'pi pi-terminal', routerLink: '/plugin/commands' },
          { label: 'Logs', icon: 'pi pi-align-left', routerLink: '/plugin/logs' },
        ],
      });
    }

    return items;
  });
}
