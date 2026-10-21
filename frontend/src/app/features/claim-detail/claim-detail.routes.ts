import { Routes } from '@angular/router';

const routes: Routes = [
  {
    path: '',
    title: 'Claim',
    loadComponent: () => import('./claim-detail.page').then((module) => module.ClaimDetailPage),
  },
];

export default routes;
