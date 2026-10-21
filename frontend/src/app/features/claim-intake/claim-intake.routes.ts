import { Routes } from '@angular/router';

const routes: Routes = [
  {
    path: '',
    title: 'New claim',
    loadComponent: () => import('./intake.page').then((module) => module.IntakePage),
  },
];

export default routes;
