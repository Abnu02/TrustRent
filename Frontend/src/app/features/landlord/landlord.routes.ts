import { Routes } from '@angular/router';

export const LANDLORD_ROUTES: Routes = [

  {
    path: '',

    loadComponent: () =>
      import('./layout/landlord-layout/landlord-layout.component')
        .then(
          m => m.LandlordLayoutComponent
        ),

    children: [

      {
        path: '',

        redirectTo: 'dashboard',

        pathMatch: 'full'
      },


      {
        path: 'dashboard',

        loadComponent: () =>
          import('./pages/dashboard/dashboard.component')
            .then(
              m => m.DashboardComponent
            )
      },


      {
        path: 'create-property',

        loadComponent: () =>
          import('./pages/create-property/create-property.component')
            .then(
              m => m.CreatePropertyComponent
            )
      },

      {
        path: 'my-properties',
        loadComponent: () =>
          import('./pages/my-properties/my-properties')
            .then(m => m.MyPropertiesComponent)
      }
    ]

  }

];