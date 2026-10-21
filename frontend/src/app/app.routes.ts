import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'claims' },
  { path: 'claims', loadChildren: () => import('./features/claims-list/claims-list.routes') },
  { path: '**', redirectTo: 'claims' },
];
