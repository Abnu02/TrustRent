import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '../../../auth/auth.service';

@Injectable({ providedIn: 'root' })
export class AdminAuthSession {
  private readonly auth = inject(AuthService);

  get displayName(): string {
    return this.auth.user()?.fullName ?? 'Admin';
  }

  get accessToken(): string | null {
    return this.auth.accessToken;
  }

  get isAdmin(): boolean {
    return this.auth.hasRole('Admin');
  }

  async signIn(email: string, password: string): Promise<void> {
    const response = await firstValueFrom(this.auth.login({ email, password }));

    if (response.user.role !== 'Admin') {
      this.auth.clearSession();
      throw new Error('Admin access is required. Sign in with an administrator account.');
    }
  }

  signOut(): void {
    this.auth.clearSession();
  }
}
