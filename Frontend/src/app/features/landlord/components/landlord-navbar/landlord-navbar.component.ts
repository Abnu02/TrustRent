import { Component, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-landlord-navbar',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './landlord-navbar.component.html',
  styleUrl: './landlord-navbar.component.scss'
})
export class LandlordNavbarComponent {
  accountMenuOpen = signal(false);

  toggleAccountMenu(): void {
    this.accountMenuOpen.update(value => !value);
  }

  closeAccountMenu(): void {
    this.accountMenuOpen.set(false);
  }

  logout(): void {
    // Authentication service will be connected later.
    console.log('Logout clicked');
  }
}