import { Routes } from '@angular/router';

export const routes: Routes = [

  {
    path: 'tenant',
    loadChildren: () =>
      import('./features/tenant/tenant.routes')
        .then(m => m.TENANT_ROUTES)
  },

  {
    path: '',
    redirectTo: 'tenant',
    pathMatch: 'full'
  }

];