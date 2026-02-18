import { ChangeDetectionStrategy, Component, computed, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { PanelMenuModule } from 'primeng/panelmenu';
import { ToolbarModule } from 'primeng/toolbar';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { MenuItem } from 'primeng/api';

type UserRole = 'designer' | 'bim_manager' | 'admin';

@Component({
  selector: 'app-main-layout',
  imports: [
    RouterOutlet,
    ButtonModule,
    PanelMenuModule,
    ToolbarModule,
    ConfirmDialogModule,
    ToastModule,
  ],
  templateUrl: './main-layout.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MainLayoutComponent {
  /**
   * Current user role. In real app, this would come from auth service.
   * For now, hardcoded to 'admin' for development.
   */
  protected readonly userRole = signal<UserRole>('admin');

  /**
   * All menu items grouped by access level.
   * PanelMenu with multiple roots - always expanded.
   */
  protected readonly menuItems = computed<MenuItem[]>(() => {
    const role = this.userRole();
    const items: MenuItem[] = [];

    // 📁 LIBRARY (all users)
    items.push({
      label: 'Library',
      icon: 'pi pi-folder-open',
      expanded: true,
      items: [
        {
          label: 'Browse',
          icon: 'pi pi-book',
          routerLink: '/library',
        },
      ],
    });

    // 🔧 WORK (BIM Manager + Admin)
    if (role === 'bim_manager' || role === 'admin') {
      items.push({
        label: 'Work',
        icon: 'pi pi-briefcase',
        expanded: true,
        items: [
          {
            label: 'Queue',
            icon: 'pi pi-list',
            routerLink: '/queue',
          },
          {
            label: 'Scanner',
            icon: 'pi pi-search',
            routerLink: '/scanner',
          },
          {
            label: 'Drafts',
            icon: 'pi pi-file-edit',
            routerLink: '/drafts',
          },
        ],
      });
    }

    // ⚙️ ADMINISTRATION (Admin only)
    if (role === 'admin') {
      items.push({
        label: 'Administration',
        icon: 'pi pi-cog',
        expanded: true,
        items: [
          {
            label: 'Dashboard',
            icon: 'pi pi-home',
            routerLink: '/dashboard',
          },
          {
            label: 'Family Roles',
            icon: 'pi pi-tags',
            routerLink: '/roles',
          },
          {
            label: 'Categories',
            icon: 'pi pi-folder',
            routerLink: '/categories',
          },
          {
            label: 'Tags',
            icon: 'pi pi-label',
            routerLink: '/tags',
          },
          {
            label: 'Recognition Rules',
            icon: 'pi pi-link',
            routerLink: '/recognition-rules',
          },
          {
            label: 'Settings',
            icon: 'pi pi-sliders-h',
            routerLink: '/settings',
          },
        ],
      });
    }

    return items;
  });
}
