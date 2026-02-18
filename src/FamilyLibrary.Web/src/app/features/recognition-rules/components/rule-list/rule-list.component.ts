import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { InputTextModule } from 'primeng/inputtext';
import { MessageService } from 'primeng/api';
import { TableModule, TableLazyLoadEvent } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';
import { RecognitionRule } from '../../../../core/models/recognition-rule.model';
import { ConfirmDialogService } from '../../../../shared/components/confirm-dialog/confirm-dialog.service';
import { RulesService } from '../../services/rules.service';
import { RuleEditorComponent } from '../rule-editor/rule-editor.component';
import { RuleTestDialogComponent } from '../rule-test-dialog/rule-test-dialog.component';
import { RolesService } from '../../../roles/services/roles.service';

@Component({
  selector: 'app-rule-list',
  imports: [
    ButtonModule,
    ConfirmDialogModule,
    FormsModule,
    InputTextModule,
    RuleEditorComponent,
    RuleTestDialogComponent,
    TableModule,
    TagModule,
    ToastModule,
    TooltipModule,
  ],
  templateUrl: './rule-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RuleListComponent {
  private readonly rulesService = inject(RulesService);
  private readonly rolesService = inject(RolesService);
  private readonly confirmDialogService = inject(ConfirmDialogService);
  private readonly messageService = inject(MessageService);

  // Pagination state
  protected readonly first = signal(0);
  protected readonly rows = signal(10);

  // Data state
  protected readonly rules = signal<RecognitionRule[]>([]);
  protected readonly totalCount = signal(0);
  protected readonly isLoading = signal(false);
  protected readonly roles = signal<Array<{ id: string; name: string }>>([]);

  // Dialog state
  protected readonly editorVisible = signal(false);
  protected readonly selectedRule = signal<RecognitionRule | null>(null);
  protected readonly testDialogVisible = signal(false);
  protected readonly testRuleId = signal<string | null>(null);

  // Deletion state
  protected readonly isDeleting = signal<string | null>(null);

  protected readonly rowsPerPageOptions = [10, 20, 50, 100];

  constructor() {
    this.loadRoles();
    this.loadRules();
  }

  private loadRoles(): void {
    this.rolesService.getRoles(1, 1000).subscribe({
      next: result => {
        this.roles.set(result.data.map(r => ({ id: r.id, name: r.name })));
      },
    });
  }

  private loadRules(): void {
    this.isLoading.set(true);

    const page = Math.floor(this.first() / this.rows()) + 1;
    const pageSize = this.rows();

    this.rulesService.getRules(page, pageSize).subscribe({
      next: result => {
        this.rules.set(result.data);
        this.totalCount.set(result.totalCount);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      },
    });
  }

  protected onPageChange(event: TableLazyLoadEvent): void {
    const first = event.first ?? 0;
    const rows = event.rows ?? 10;
    this.first.set(first);
    this.rows.set(rows);
    this.loadRules();
  }

  protected createRule(): void {
    this.selectedRule.set(null);
    this.editorVisible.set(true);
  }

  protected editRule(rule: RecognitionRule): void {
    this.selectedRule.set(rule);
    this.editorVisible.set(true);
  }

  protected testRule(rule: RecognitionRule): void {
    this.testRuleId.set(rule.id);
    this.testDialogVisible.set(true);
  }

  protected deleteRule(rule: RecognitionRule, event: Event): void {
    this.confirmDialogService
      .delete(
        'Are you sure you want to delete this recognition rule?',
        event.currentTarget as EventTarget,
      )
      .subscribe(confirmed => {
        if (confirmed) {
          this.performDelete(rule.id);
        }
      });
  }

  private async performDelete(id: string): Promise<void> {
    this.isDeleting.set(id);

    this.rulesService.deleteRule(id).subscribe({
      next: () => {
        this.rules.update(rules => rules.filter(r => r.id !== id));
        this.isDeleting.set(null);
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Rule deleted successfully',
        });
      },
      error: () => {
        this.isDeleting.set(null);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to delete rule',
        });
      },
    });
  }

  protected onEditorSaved(): void {
    this.editorVisible.set(false);
    this.selectedRule.set(null);
    this.loadRules();
    this.messageService.add({
      severity: 'success',
      summary: 'Success',
      detail: 'Rule saved successfully',
    });
  }

  protected onEditorClosed(): void {
    this.editorVisible.set(false);
    this.selectedRule.set(null);
  }

  protected onTestDialogClosed(): void {
    this.testDialogVisible.set(false);
    this.testRuleId.set(null);
  }

  protected refreshRules(): void {
    this.loadRules();
  }

  protected getRoleName(roleId: string): string {
    const role = this.roles().find(r => r.id === roleId);
    return role?.name ?? roleId;
  }
}
