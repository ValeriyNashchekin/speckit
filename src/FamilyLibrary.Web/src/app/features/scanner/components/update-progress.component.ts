import { ChangeDetectionStrategy, Component, input, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProgressBarModule } from 'primeng/progressbar';
import { ButtonModule } from 'primeng/button';

/**
 * Progress payload structure
 */
interface UpdateProgress {
  completed: number;
  total: number;
  currentFamily: string;
  success: number;
  failed: number;
}

/**
 * Modal component showing update progress with progress bar.
 */
@Component({
  selector: 'app-update-progress',
  imports: [CommonModule, ProgressBarModule, ButtonModule],
  templateUrl: './update-progress.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UpdateProgressComponent {
  // Inputs
  progress = input<UpdateProgress | null>(null);
  visible = input<boolean>(true);

  // Computed values
  protected readonly progressPercent = computed(() => {
    const p = this.progress();
    if (!p || p.total === 0) return 0;
    return Math.round((p.completed / p.total) * 100);
  });

  protected readonly hasFailures = computed(() => {
    const p = this.progress();
    return p ? p.failed > 0 : false;
  });

  protected readonly isComplete = computed(() => {
    const p = this.progress();
    return p ? p.completed >= p.total : false;
  });
}
