import { Routes } from '@angular/router';

export const ROLES_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./components/role-list/role-list.component').then(m => m.RoleListComponent),
  },
  {
    path: 'new',
    loadComponent: () =>
      import('./components/role-editor/role-editor.component').then(m => m.RoleEditorComponent),
  },
  {
    path: ':id/edit',
    loadComponent: () =>
      import('./components/role-editor/role-editor.component').then(m => m.RoleEditorComponent),
  },
];
