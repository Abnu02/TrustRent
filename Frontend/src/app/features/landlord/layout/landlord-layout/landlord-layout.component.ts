import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import { LandlordNavbarComponent } from '../../components/landlord-navbar/landlord-navbar.component';
import { LandlordSidebarComponent } from '../../components/landlord-sidebar/landlord-sidebar.component';

@Component({
  selector: 'app-landlord-layout',
  standalone: true,
  imports: [
    RouterOutlet,
    LandlordNavbarComponent,
    LandlordSidebarComponent
  ],
  templateUrl: './landlord-layout.component.html',
  styleUrl: './landlord-layout.component.scss'
})
export class LandlordLayoutComponent {

  /**
   * Controls whether the sidebar is visible.
   *
   * true  = sidebar visible
   * false = sidebar hidden
   */
  sidebarOpen = signal(true);

  /**
   * Opens or closes the sidebar.
   */
  toggleSidebar(): void {
    this.sidebarOpen.update(value => !value);
  }
}