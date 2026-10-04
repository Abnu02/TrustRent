import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../auth/auth.service';

@Component({
  selector: 'app-tenant-coming-soon',
  standalone: true,
  templateUrl: './tenant-coming-soon.component.html',
  styleUrl: './tenant-coming-soon.component.scss',
})
export class TenantComingSoonComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  readonly user = this.auth.user;

  signOut(): void {
    this.auth.clearSession();
    void this.router.navigateByUrl('/auth');
  }
}
