import { Routes } from '@angular/router';
import { AdminLayoutComponent } from './layout/admin-layout/admin-layout';

export const ADMIN_ROUTES: Routes = [
  {
    path: '',
    component: AdminLayoutComponent,

    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      },

      {
        path: 'dashboard',
        loadComponent: () =>
          import('./dashboard/admin-dashboard')
            .then(m => m.AdminDashboardComponent)
      },

      {
        path: 'landlords',
        loadComponent: () =>
          import('./landlords/pending-landlords')
            .then(m => m.LandlordManagementComponent)
      },

      {
        path: 'properties',
        loadComponent: () =>
          import('./properties/pending-properties')
            .then(m => m.PendingPropertiesComponent)
      }
    ]
  }
];