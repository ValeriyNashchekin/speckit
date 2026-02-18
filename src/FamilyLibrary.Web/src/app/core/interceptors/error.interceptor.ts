import { HttpErrorResponse, HttpHandlerFn, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { MessageService } from 'primeng/api';
import { catchError, throwError } from 'rxjs';

interface ValidationError {
  property: string;
  message: string;
}

interface ApiErrorResponse {
  title?: string;
  status: number;
  detail?: string;
  errors?: ValidationError[];
}

export const errorInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn) => {
  const messageService = inject(MessageService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      const apiError = error.error as ApiErrorResponse | undefined;
      let errorMessage = 'An unexpected error occurred';

      switch (error.status) {
        case 0:
          errorMessage = 'Unable to connect to server. Please check your connection.';
          break;
        case 400:
          if (apiError?.errors && apiError.errors.length > 0) {
            errorMessage = apiError.errors.map(e => `${e.property}: ${e.message}`).join('\n');
          } else {
            errorMessage = apiError?.detail || apiError?.title || 'Invalid request';
          }
          break;
        case 401:
          errorMessage = 'Your session has expired. Please log in again.';
          break;
        case 403:
          errorMessage = 'You do not have permission to perform this action.';
          break;
        case 404:
          errorMessage = apiError?.detail || 'The requested resource was not found.';
          break;
        case 409:
          errorMessage = apiError?.detail || 'A conflict occurred with the current state.';
          break;
        case 422:
          if (apiError?.errors && apiError.errors.length > 0) {
            errorMessage = apiError.errors.map(e => e.message).join('\n');
          } else {
            errorMessage = 'Validation failed';
          }
          break;
        case 500:
          errorMessage = 'An internal server error occurred. Please try again later.';
          break;
        case 503:
          errorMessage = 'Service temporarily unavailable. Please try again later.';
          break;
        default:
          errorMessage = apiError?.detail || apiError?.title || error.message || 'An error occurred';
      }

      messageService.add({
        severity: 'error',
        summary: `Error ${error.status}`,
        detail: errorMessage,
        life: 5000,
      });

      return throwError(() => error);
    }),
  );
};
