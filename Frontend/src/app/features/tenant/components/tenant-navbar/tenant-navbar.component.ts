import {
  Component,
  HostListener,
  inject,
  signal
} from '@angular/core';
import { AuthService } from '../../../../core/auth/auth.service';

@Component({
  selector: 'app-tenant-navbar',
  standalone: true,
  templateUrl: './tenant-navbar.component.html',
  styleUrl: './tenant-navbar.component.scss'
})
export class TenantNavbarComponent {

  private readonly authService = inject(AuthService);
  readonly menuOpen = signal(false);

  get user() {
    return this.authService.currentUser();
  }

  toggleMenu(): void {
    this.menuOpen.update(value => !value);
  }

  logout(): void {
    this.menuOpen.set(false);
    this.authService.logout();
  }

  @HostListener('document:click', ['$event'])
  closeMenu(event: MouseEvent): void {

    const target = event.target as HTMLElement;

    if (!target.closest('.profile-area')) {
      this.menuOpen.set(false);
    }
  }
}