import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/auth/auth.service';

@Component({
  selector: 'app-landlord-navbar',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './landlord-navbar.component.html',
  styleUrl: './landlord-navbar.component.scss'
})
export class LandlordNavbarComponent {
  private readonly authService = inject(AuthService);

  accountMenuOpen = signal(false);

  toggleAccountMenu(): void {
    this.accountMenuOpen.update(value => !value);
  }

  closeAccountMenu(): void {
    this.accountMenuOpen.set(false);
  }

  logout(): void {
    this.accountMenuOpen.set(false);
    this.authService.logout();
  }

  get currentUser() {
    return this.authService.currentUser();
  }
}