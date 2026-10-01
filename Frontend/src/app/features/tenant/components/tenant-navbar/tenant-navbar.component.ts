import {
  Component,
  HostListener,
  signal
} from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-tenant-navbar',
  standalone: true,
  templateUrl: './tenant-navbar.component.html',
  styleUrl: './tenant-navbar.component.scss'
})
export class TenantNavbarComponent {

  readonly menuOpen = signal(false);

  readonly user = {
    fullName: 'Alex',
    email: 'alex@example.com',
    role: 'Tenant',
    isVerified: true
  };

  constructor(
    private readonly router: Router
  ) {}

  toggleMenu(): void {
    this.menuOpen.update(value => !value);
  }

  logout(): void {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('user');

    this.router.navigate(['/login']);
  }

  @HostListener('document:click', ['$event'])
  closeMenu(event: MouseEvent): void {

    const target = event.target as HTMLElement;

    if (!target.closest('.profile-area')) {
      this.menuOpen.set(false);
    }
  }
}