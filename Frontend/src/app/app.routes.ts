import { Routes } from '@angular/router';
import { landlordGuard } from './auth/landlord.guard';
import { tenantGuard } from './auth/tenant.guard';
import { Auth } from './features/auth/auth';
import { AdminLayout } from './features/Admin/admin-layout';
import { AdminOverview } from './features/Admin/pages/overview/overview';
import { PropertyReviews } from './features/Admin/pages/property-reviews/property-reviews';
import { LandlordReviews } from './features/Admin/pages/landlord-reviews/landlord-reviews';
import { AuditLog } from './features/Admin/pages/audit-log/audit-log';
import { PropertyDetail } from './features/Admin/pages/property-detail/property-detail';
import { adminAuthGuard } from './features/Admin/services/admin-auth.guard';
import { LandlordLayoutComponent } from './features/Landlord/landlord-layout.component';
import { DashboardComponent } from './features/Landlord/pages/dashboard/dashboard.component';
import { CreatePropertyComponent } from './features/Landlord/pages/create-property/create-property.component';
import { EditPropertyComponent } from './features/Landlord/pages/edit-property/edit-property.component';
import { PropertyDetailsComponent } from './features/Landlord/pages/property-details/property-details.component';
import { PropertyListComponent } from './features/Landlord/pages/property-list/property-list.component';
import { TenantComingSoonComponent } from './pages/tenant-coming-soon/tenant-coming-soon.component';

export const routes: Routes = [
  { path: '', component: Auth },
  { path: 'auth', component: Auth },
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
  {
    path: 'landlord',
    component: LandlordLayoutComponent,
    canActivate: [landlordGuard],
    children: [
      { path: '', pathMatch: 'full', component: DashboardComponent },
      { path: 'dashboard', component: DashboardComponent },
      { path: 'properties/new', component: CreatePropertyComponent },
      { path: 'properties/:id/edit', component: EditPropertyComponent },
      { path: 'properties/:id', component: PropertyDetailsComponent },
      { path: 'properties', component: PropertyListComponent },
    ],
  },
  {
    path: 'tenant',
    component: TenantComingSoonComponent,
    canActivate: [tenantGuard],
  },
  { path: '**', redirectTo: '' },
];
