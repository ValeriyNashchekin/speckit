import { Routes } from '@angular/router';

export const SETTINGS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./settings.component').then((m) => m.SettingsComponent),
    children: [
      {
        path: '',
        redirectTo: 'mappings',
        pathMatch: 'full',
      },
      {
        path: 'mappings',
        loadComponent: () =>
          import('./components/material-mappings/material-mappings-list.component').then(
            (m) => m.MaterialMappingsListComponent
          ),
      },
    ],
  },
];
