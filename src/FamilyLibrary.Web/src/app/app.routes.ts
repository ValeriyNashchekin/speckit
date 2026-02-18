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
        path: 'library',
        loadChildren: () =>
          import('./features/library/library.routes').then(m => m.LIBRARY_ROUTES),
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
        path: 'recognition-rules',
        loadChildren: () =>
          import('./features/recognition-rules/recognition-rules.routes').then(
            m => m.RECOGNITION_RULES_ROUTES,
          ),
      },
      {
        path: 'queue',
        loadChildren: () =>
          import('./features/queue/queue.routes').then(m => m.QUEUE_ROUTES),
      },
      {
        path: 'scanner',
        loadChildren: () =>
          import('./features/scanner/scanner.routes').then((m) => m.SCANNER_ROUTES),
      },
      {
        path: 'settings',
        loadChildren: () =>
          import('./features/settings/settings.routes').then((m) => m.SETTINGS_ROUTES),
      },
    ],
  },
  {
    path: '**',
    redirectTo: '/dashboard',
  },
];
