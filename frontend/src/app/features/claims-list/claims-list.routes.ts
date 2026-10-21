import { Routes } from '@angular/router';

const routes: Routes = [
  {
    path: '',
    title: 'Claims',
    loadComponent: () => import('./claims-list.page').then((module) => module.ClaimsListPage),
  },
];

export default routes;
