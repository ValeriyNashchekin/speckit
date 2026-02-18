import { Routes } from '@angular/router';

export const RECOGNITION_RULES_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./components/rule-list/rule-list.component').then(
        m => m.RuleListComponent,
      ),
  },
];
