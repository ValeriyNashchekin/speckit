import { Routes } from '@angular/router';

export const FAMILIES_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./components/family-list/family-list.component').then(m => m.FamilyListComponent),
  },
];
