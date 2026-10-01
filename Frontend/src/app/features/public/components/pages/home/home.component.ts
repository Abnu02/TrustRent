import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

import { PublicNavbarComponent } from '../../public-navbar/public-navbar.component';
import { PublicFooterComponent } from '../../public-footer/public-footer.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    RouterLink,
    PublicNavbarComponent,
    PublicFooterComponent
  ],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent {

  search(): void {
    // Later this can navigate to the tenant/property listing page.
    console.log('Search verified rentals');
  }

}