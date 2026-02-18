import { Routes } from '@angular/router';

export const TAGS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./components/tag-list/tag-list.component').then(m => m.TagListComponent),
  },
];
