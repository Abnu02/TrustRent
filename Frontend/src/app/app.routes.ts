import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadChildren: () =>
      import('./features/public/public.routes')
        .then(m => m.PUBLIC_ROUTES)
  },

  {
    path: 'tenant',
    loadChildren: () =>
      import('./features/tenant/tenant.routes')
        .then(m => m.TENANT_ROUTES)
  },

  // {
  //   path: 'login',
  //   loadComponent: () =>
  //     import('./features/auth/pages/login/login.component')
  //       .then(m => m.LoginComponent)
  // },

  {
    path: '**',
    redirectTo: ''
  }
];