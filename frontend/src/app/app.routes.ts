import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'claims' },
  { path: 'claims/new', loadChildren: () => import('./features/claim-intake/claim-intake.routes') },
  { path: 'claims/:claimId', loadChildren: () => import('./features/claim-detail/claim-detail.routes') },
  { path: 'claims', loadChildren: () => import('./features/claims-list/claims-list.routes') },
  { path: '**', redirectTo: 'claims' },
];
