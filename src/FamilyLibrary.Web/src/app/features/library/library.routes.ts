import { Routes } from '@angular/router';

export const LIBRARY_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./components/library/library.component').then(m => m.LibraryComponent),
  },
  {
    path: ':id',
    loadComponent: () =>
      import('./components/family-detail/family-detail.component').then(m => m.FamilyDetailComponent),
  },
];
