import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./layout/main-layout/main-layout.component').then(m => m.MainLayoutComponent),
    children: [
      {
        path: '',
        redirectTo: '/library',
        pathMatch: 'full',
      },
      {
        path: 'library',
        loadChildren: () =>
          import('./features/library/library.routes').then(m => m.LIBRARY_ROUTES),
      },
      {
        path: 'scanner',
        loadChildren: () =>
          import('./features/scanner/scanner.routes').then(m => m.SCANNER_ROUTES),
      },
      {
        path: 'queue',
        loadChildren: () =>
          import('./features/queue/queue.routes').then(m => m.QUEUE_ROUTES),
      },
      {
        path: 'roles',
        loadChildren: () =>
          import('./features/roles/roles.routes').then(m => m.ROLES_ROUTES),
      },
      {
        path: 'recognition-rules',
        loadChildren: () =>
          import('./features/recognition-rules/recognition-rules.routes').then(
            m => m.RECOGNITION_RULES_ROUTES,
          ),
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
        path: 'settings',
        loadComponent: () =>
          import('./features/settings/settings.component').then(m => m.SettingsComponent),
      },
    ],
  },
  {
    path: '**',
    redirectTo: '/library',
  },
];
