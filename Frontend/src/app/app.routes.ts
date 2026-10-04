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

export const routes: Routes = [];
