import { Routes } from '@angular/router';

export const DRAFTS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./components/draft-list/draft-list.component').then(m => m.DraftListComponent),
  },
];
