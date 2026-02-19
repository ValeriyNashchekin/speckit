import { Routes } from '@angular/router';

export const PLUGIN_ROUTES: Routes = [
  {
    path: 'commands',
    loadComponent: () =>
      import('./commands/commands.component').then(m => m.CommandsComponent),
  },
  {
    path: 'logs',
    loadComponent: () =>
      import('./logs/logs.component').then(m => m.LogsComponent),
  },
];
