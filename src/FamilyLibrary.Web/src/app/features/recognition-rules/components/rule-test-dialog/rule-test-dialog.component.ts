import {
  ChangeDetectionStrategy,
  Component,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { RulesService, TestResult } from '../../services/rules.service';

@Component({
  selector: 'app-rule-test-dialog',
  imports: [ButtonModule, DialogModule, FormsModule, InputTextModule, TagModule],
  templateUrl: './rule-test-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RuleTestDialogComponent {
  // inputs
  visible = input<boolean>(false);
  ruleId = input<string | null>(null);

  // outputs
  closed = output<void>();

  // state
  protected readonly familyName = signal('');
  protected readonly isTesting = signal(false);
  protected readonly testResult = signal<TestResult | null>(null);

  // services
  private readonly rulesService = inject(RulesService);

  protected get canTest(): boolean {
    return this.familyName().trim().length > 0 && this.ruleId() !== null;
  }

  protected onTest(): void {
    const ruleId = this.ruleId();
    const name = this.familyName().trim();

    if (!ruleId || !name) return;

    this.isTesting.set(true);
    this.testResult.set(null);

    this.rulesService.testRule(ruleId, name).subscribe({
      next: result => {
        this.testResult.set(result);
        this.isTesting.set(false);
      },
      error: () => {
        this.isTesting.set(false);
        this.testResult.set({
          matches: false,
          matchedConditions: [],
        });
      },
    });
  }

  protected onHide(): void {
    this.closed.emit();
    this.reset();
  }

  private reset(): void {
    this.familyName.set('');
    this.testResult.set(null);
    this.isTesting.set(false);
  }
}
