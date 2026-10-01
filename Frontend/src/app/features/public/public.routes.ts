import { Routes } from '@angular/router';

export const PUBLIC_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./components/pages/home/home.component')
        .then(m => m.HomeComponent)
  }
];