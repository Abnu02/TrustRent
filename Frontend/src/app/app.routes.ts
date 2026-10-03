import { Routes } from '@angular/router';
import { authGuard, roleGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  // Public website
  {
    path: '',
    loadChildren: () =>
      import('./features/public/public.routes')
        .then(m => m.PUBLIC_ROUTES)
  },

  // Authentication
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login/login')
        .then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./features/auth/register/register')
        .then(m => m.RegisterComponent)
  },

  // Tenant
  // Tenant does NOT require verification.
  {
    path: 'tenant',
    canActivate: [
      authGuard,
      roleGuard(['Tenant'])
    ],
    loadChildren: () =>
      import('./features/tenant/tenant.routes')
        .then(m => m.TENANT_ROUTES)
  },

  // Landlord
  // Landlord must be authenticated.
  // Verification is enforced by the backend before
  // the landlord can create/manage properties.
  {
    path: 'landlord',
    canActivate: [
      authGuard,
      roleGuard(['Landlord'])
    ],
    loadChildren: () =>
      import('./features/landlord/landlord.routes')
        .then(m => m.LANDLORD_ROUTES)
  },

  // // Admin
  // {
  //   path: 'admin',
  //   canActivate: [
  //     authGuard,
  //     roleGuard(['Admin'])
  //   ],
  //   loadChildren: () =>
  //     import('./features/admin/admin.routes')
  //       .then(m => m.ADMIN_ROUTES)
  // },

  // Unknown route
  {
    path: '**',
    redirectTo: ''
  }
];