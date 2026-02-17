import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { MenuModule } from 'primeng/menu';
import { ToolbarModule } from 'primeng/toolbar';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';

interface NavItem {
  label: string;
  icon: string;
  route: string;
}

@Component({
  selector: 'app-main-layout',
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    ButtonModule,
    MenuModule,
    ToolbarModule,
    ConfirmDialogModule,
    ToastModule,
  ],
  templateUrl: './main-layout.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MainLayoutComponent {
  protected readonly navItems = signal<NavItem[]>([
    { label: 'Dashboard', icon: 'pi pi-home', route: '/dashboard' },
    { label: 'Queue', icon: 'pi pi-list', route: '/queue' },
    { label: 'Family Roles', icon: 'pi pi-tags', route: '/roles' },
    { label: 'Categories', icon: 'pi pi-folder', route: '/categories' },
    { label: 'Tags', icon: 'pi pi-label', route: '/tags' },
    { label: 'Families', icon: 'pi pi-box', route: '/families' },
    { label: 'Drafts', icon: 'pi pi-file-edit', route: '/drafts' },
    { label: 'Settings', icon: 'pi pi-cog', route: '/settings' },
  ]);
}
