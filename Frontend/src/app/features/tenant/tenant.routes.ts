import { Routes } from '@angular/router';

export const TENANT_ROUTES: Routes = [

  {
    path: '',
    loadComponent: () =>
      import('./pages/tenant-dashboard/tenant-dashboard.component')
        .then(m => m.TenantDashboardComponent)
  },

  {
    path: 'properties/:id',
    loadComponent: () =>
      import('./pages/property-details/property-details.component')
        .then(m => m.PropertyDetailsComponent)
  }

];