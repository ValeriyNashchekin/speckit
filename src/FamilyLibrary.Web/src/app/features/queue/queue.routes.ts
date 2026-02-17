import { Routes } from '@angular/router';

export const QUEUE_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./components/queue/queue.component').then(m => m.QueueComponent),
  },
];
