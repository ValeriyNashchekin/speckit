import { inject, Injectable } from '@angular/core';
import { ConfirmationService, ConfirmEventType } from 'primeng/api';
import { Observable } from 'rxjs';

export interface ConfirmOptions {
  header?: string;
  message: string;
  icon?: string;
  acceptLabel?: string;
  rejectLabel?: string;
  acceptSeverity?: 'primary' | 'secondary' | 'success' | 'info' | 'warning' | 'danger' | 'contrast';
  rejectSeverity?: 'primary' | 'secondary' | 'success' | 'info' | 'warning' | 'danger' | 'contrast';
}

@Injectable({
  providedIn: 'root',
})
export class ConfirmDialogService {
  private readonly confirmationService = inject(ConfirmationService);

  confirm(options: ConfirmOptions, target?: EventTarget): Observable<boolean> {
    return new Observable(observer => {
      this.confirmationService.confirm({
        target: target,
        header: options.header ?? 'Confirmation',
        message: options.message,
        icon: options.icon ?? 'pi pi-exclamation-triangle',
        acceptLabel: options.acceptLabel ?? 'Yes',
        rejectLabel: options.rejectLabel ?? 'No',
        acceptButtonProps: {
          severity: options.acceptSeverity ?? 'primary',
        },
        rejectButtonProps: {
          severity: options.rejectSeverity ?? 'secondary',
          outlined: true,
        },
        accept: () => {
          observer.next(true);
          observer.complete();
        },
        reject: (type: ConfirmEventType) => {
          observer.next(false);
          observer.complete();
        },
      });
    });
  }

  delete(message?: string, target?: EventTarget): Observable<boolean> {
    return this.confirm(
      {
        header: 'Delete Confirmation',
        message: message ?? 'Are you sure you want to delete this item? This action cannot be undone.',
        icon: 'pi pi-exclamation-triangle',
        acceptLabel: 'Delete',
        acceptSeverity: 'danger',
        rejectLabel: 'Cancel',
      },
      target,
    );
  }

  save(message?: string, target?: EventTarget): Observable<boolean> {
    return this.confirm(
      {
        header: 'Save Changes',
        message: message ?? 'Do you want to save your changes?',
        icon: 'pi pi-save',
        acceptLabel: 'Save',
        acceptSeverity: 'success',
        rejectLabel: 'Discard',
      },
      target,
    );
  }
}
