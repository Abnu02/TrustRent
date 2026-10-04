import { Routes } from '@angular/router';
import { landlordGuard } from './auth/landlord.guard';
import { Auth } from './features/auth/auth';
import { AdminLayout } from './features/Admin/admin-layout';
import { AdminOverview } from './features/Admin/pages/overview/overview';
import { PropertyReviews } from './features/Admin/pages/property-reviews/property-reviews';
import { LandlordReviews } from './features/Admin/pages/landlord-reviews/landlord-reviews';
import { AuditLog } from './features/Admin/pages/audit-log/audit-log';
import { PropertyDetail } from './features/Admin/pages/property-detail/property-detail';
import { adminAuthGuard } from './features/Admin/services/admin-auth.guard';
import { LandlordLayoutComponent } from './layouts/landlord-layout.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { CreatePropertyComponent } from './pages/create-property/create-property.component';
import { EditPropertyComponent } from './pages/edit-property/edit-property.component';
import { PropertyDetailsComponent } from './pages/property-details/property-details.component';
import { PropertyListComponent } from './pages/property-list/property-list.component';

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
  { path: '**', redirectTo: '' },
];
