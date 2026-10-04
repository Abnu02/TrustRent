import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';

import { AuthService } from '../../../../core/auth/auth.service';

@Component({
  selector: 'app-admin-navbar',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './admin-navbar.html',
  styleUrl: './admin-navbar.scss'
})
export class AdminNavbarComponent {

  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  accountOpen = signal(false);

  toggleAccount(): void {
    this.accountOpen.update(open => !open);
  }

  logout(): void {
    this.accountOpen.set(false);
    this.authService.logout();
  }

  get currentUser() {
    return this.authService.currentUser();
  }
}