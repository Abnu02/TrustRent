import { Routes } from '@angular/router';
import { Auth } from './features/auth/auth';
import { AdminLayout } from './features/Admin/admin-layout';
import { AdminOverview } from './features/Admin/pages/overview/overview';
import { PropertyReviews } from './features/Admin/pages/property-reviews/property-reviews';
import { LandlordReviews } from './features/Admin/pages/landlord-reviews/landlord-reviews';
import { AuditLog } from './features/Admin/pages/audit-log/audit-log';
import { PropertyDetail } from './features/Admin/pages/property-detail/property-detail';
import { adminAuthGuard } from './features/Admin/services/admin-auth.guard';

export const routes: Routes = [
  { path: '', component: Auth },
  {
    path: 'admin',
    component: AdminLayout,
    canActivate: [adminAuthGuard],
    canActivateChild: [adminAuthGuard],
    children: [
      { path: '', pathMatch: 'full', component: AdminOverview },
      { path: 'properties/:id', component: PropertyDetail },
      { path: 'properties', component: PropertyReviews },
      { path: 'landlords', component: LandlordReviews },
      { path: 'audit-log', component: AuditLog },
    ],
  },
  { path: '**', redirectTo: '' },
];
