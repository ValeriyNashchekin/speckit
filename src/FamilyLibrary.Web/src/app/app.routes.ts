import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./layout/main-layout/main-layout.component').then(m => m.MainLayoutComponent),
    children: [
      {
        path: '',
        redirectTo: '/dashboard',
        pathMatch: 'full',
      },
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent),
      },
      {
        path: 'roles',
        loadChildren: () =>
          import('./features/roles/roles.routes').then(m => m.ROLES_ROUTES),
      },
      {
        path: 'categories',
        loadChildren: () =>
          import('./features/categories/categories.routes').then(m => m.CATEGORIES_ROUTES),
      },
      {
        path: 'tags',
        loadChildren: () =>
          import('./features/tags/tags.routes').then(m => m.TAGS_ROUTES),
      },
      {
        path: 'families',
        loadChildren: () =>
          import('./features/families/families.routes').then(m => m.FAMILIES_ROUTES),
      },
      {
        path: 'drafts',
        loadChildren: () =>
          import('./features/drafts/drafts.routes').then(m => m.DRAFTS_ROUTES),
      },
      {
        path: 'settings',
        loadComponent: () =>
          import('./features/settings/settings.component').then(m => m.SettingsComponent),
      },
    ],
  },
  {
    path: '**',
    redirectTo: '/dashboard',
  },
];
