import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { CardModule } from 'primeng/card';
import { SkeletonModule } from 'primeng/skeleton';
import { QueueService, LibraryStatistics } from '../../services/queue.service';

@Component({
  selector: 'app-library-status',
  imports: [CardModule, SkeletonModule],
  templateUrl: './library-status.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LibraryStatusComponent implements OnInit {
  private readonly queueService = inject(QueueService);

  protected readonly statistics = signal<LibraryStatistics | null>(null);
  protected readonly loading = signal(true);

  ngOnInit(): void {
    this.loadStatistics();
  }

  private loadStatistics(): void {
    this.loading.set(true);
    this.queueService.getStatistics().subscribe({
      next: stats => {
        this.statistics.set(stats);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      },
    });
  }

  protected getRoleEntries(stats: LibraryStatistics): { key: string; value: number }[] {
    return Object.entries(stats.familiesByRole).map(([key, value]) => ({ key, value }));
  }
}
