import { Routes } from '@angular/router';
import { landlordGuard } from './auth/landlord.guard';

export const routes: Routes = [
	{
		path: 'auth',
		loadComponent: () => import('./features/auth/auth').then(module => module.Auth),
	},
	{
		path: 'landlord',
		canActivate: [landlordGuard],
		loadComponent: () => import('./layouts/landlord-layout.component')
			.then(module => module.LandlordLayoutComponent),
		children: [
			{ path: '', pathMatch: 'full', redirectTo: 'dashboard' },
			{
				path: 'dashboard',
				loadComponent: () => import('./pages/dashboard/dashboard.component')
					.then(module => module.DashboardComponent),
			},
			{
				path: 'properties/new',
				loadComponent: () => import('./pages/create-property/create-property.component')
					.then(module => module.CreatePropertyComponent),
			},
			{
				path: 'properties/:id/edit',
				loadComponent: () => import('./pages/edit-property/edit-property.component')
					.then(module => module.EditPropertyComponent),
			},
			{
				path: 'properties/:id',
				loadComponent: () => import('./pages/property-details/property-details.component')
					.then(module => module.PropertyDetailsComponent),
			},
			{
				path: 'properties',
				loadComponent: () => import('./pages/property-list/property-list.component')
					.then(module => module.PropertyListComponent),
			},
		],
	},
	{ path: '', pathMatch: 'full', redirectTo: 'auth' },
	{ path: '**', redirectTo: 'auth' },
];
