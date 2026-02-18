import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { QueueService } from '../queue/services/queue.service';
import { RolesService } from '../roles/services/roles.service';
import { CategoriesService } from '../categories/services/categories.service';
import { forkJoin } from 'rxjs';

interface StatCard {
  label: string;
  value: number;
  icon: string;
  color: string;
  bg: string;
  route: string;
}

@Component({
  selector: 'app-dashboard',
  imports: [RouterLink],
  template: `
    <div class="p-5 space-y-5">
      <div>
        <h1 class="text-lg font-semibold text-[var(--text-primary)]">Dashboard</h1>
        <p class="text-xs text-[var(--text-muted)] mt-0.5">Family Library overview</p>
      </div>

      @if (loading()) {
        <div class="grid grid-cols-2 lg:grid-cols-4 gap-3">
          @for (i of [1,2,3,4]; track i) {
            <div class="bg-white rounded-lg border border-[var(--border-subtle)] p-4">
              <div class="h-4 bg-gray-200 rounded animate-pulse w-20 mb-2"></div>
              <div class="h-6 bg-gray-200 rounded animate-pulse w-12"></div>
            </div>
          }
        </div>
      } @else {
        <div class="grid grid-cols-2 lg:grid-cols-4 gap-3">
          @for (card of cards(); track card.label) {
            <a
              [routerLink]="card.route"
              class="bg-white rounded-lg border border-[var(--border-subtle)] p-4 hover:border-[var(--accent)] hover:shadow-sm transition-all group"
            >
              <div class="flex items-center gap-3">
                <div class="w-9 h-9 rounded-lg flex items-center justify-center" [style.background]="card.bg">
                  <i [class]="card.icon" [style.color]="card.color" class="text-sm"></i>
                </div>
                <div>
                  <p class="text-[0.6875rem] text-[var(--text-muted)]">{{ card.label }}</p>
                  <p class="text-lg font-semibold text-[var(--text-primary)]">{{ card.value }}</p>
                </div>
              </div>
            </a>
          }
        </div>
      }
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardComponent {
  private readonly queueService = inject(QueueService);
  private readonly rolesService = inject(RolesService);
  private readonly categoriesService = inject(CategoriesService);

  protected readonly loading = signal(true);
  protected readonly cards = signal<StatCard[]>([
    { label: 'Families', value: 0, icon: 'pi pi-objects-column', color: '#16a34a', bg: '#f0fdf4', route: '/library' },
    { label: 'Roles', value: 0, icon: 'pi pi-id-card', color: '#4f6ef7', bg: '#eef1fe', route: '/roles' },
    { label: 'Pending Drafts', value: 0, icon: 'pi pi-inbox', color: '#ea580c', bg: '#fff7ed', route: '/queue' },
    { label: 'Categories', value: 0, icon: 'pi pi-sitemap', color: '#9333ea', bg: '#faf5ff', route: '/categories' },
  ]);

  constructor() {
    this.loadStats();
  }

  private loadStats(): void {
    forkJoin({
      stats: this.queueService.getStatistics(),
      roles: this.rolesService.getRoles(1, 1),
      categories: this.categoriesService.getCategories(),
    }).subscribe({
      next: ({ stats, roles, categories }) => {
        this.cards.set([
          { label: 'Families', value: stats.totalFamilies, icon: 'pi pi-objects-column', color: '#16a34a', bg: '#f0fdf4', route: '/library' },
          { label: 'Roles', value: roles.totalCount, icon: 'pi pi-id-card', color: '#4f6ef7', bg: '#eef1fe', route: '/roles' },
          { label: 'Pending Drafts', value: stats.pendingDrafts, icon: 'pi pi-inbox', color: '#ea580c', bg: '#fff7ed', route: '/queue' },
          { label: 'Categories', value: categories.length, icon: 'pi pi-sitemap', color: '#9333ea', bg: '#faf5ff', route: '/categories' },
        ]);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      },
    });
  }
}
