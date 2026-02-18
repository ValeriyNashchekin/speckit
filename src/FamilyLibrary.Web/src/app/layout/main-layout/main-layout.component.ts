import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';

interface NavItem {
  label: string;
  icon: string;
  route: string;
}

interface NavGroup {
  title: string;
  items: NavItem[];
}

@Component({
  selector: 'app-main-layout',
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    ConfirmDialogModule,
    ToastModule,
    TooltipModule,
  ],
  templateUrl: './main-layout.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MainLayoutComponent {
  protected readonly collapsed = signal(false);

  protected readonly navGroups = signal<NavGroup[]>([
    {
      title: '',
      items: [
        { label: 'Library', icon: 'pi pi-objects-column', route: '/library' },
        { label: 'Scanner', icon: 'pi pi-sync', route: '/scanner' },
      ],
    },
    {
      title: 'Manage',
      items: [
        { label: 'Queue', icon: 'pi pi-inbox', route: '/queue' },
        { label: 'Roles', icon: 'pi pi-id-card', route: '/roles' },
        { label: 'Rules', icon: 'pi pi-microchip-ai', route: '/recognition-rules' },
        { label: 'Name Mapping', icon: 'pi pi-link', route: '/name-mapping' },
      ],
    },
    {
      title: 'Configure',
      items: [
        { label: 'Categories', icon: 'pi pi-sitemap', route: '/categories' },
        { label: 'Tags', icon: 'pi pi-hashtag', route: '/tags' },
      ],
    },
  ]);

  protected toggleSidebar(): void {
    this.collapsed.update(v => !v);
  }
}
