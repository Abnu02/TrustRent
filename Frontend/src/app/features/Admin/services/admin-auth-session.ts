import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

interface LoginResponse {
  accessToken: string;
  user: {
    fullName: string;
    role: string;
  };
}

@Injectable({ providedIn: 'root' })
export class AdminAuthSession {
  private readonly http = inject(HttpClient);
  private readonly tokenKey = 'trustrent.access-token';
  private readonly roleKey = 'trustrent.user-role';
  private readonly nameKey = 'trustrent.user-name';

  get displayName(): string {
    return sessionStorage.getItem(this.nameKey) ?? 'Admin';
  }

  get accessToken(): string | null {
    return sessionStorage.getItem(this.tokenKey);
  }

  get isAdmin(): boolean {
    return this.getRole() === 'Admin' && this.accessToken !== null;
  }

  async signIn(email: string, password: string): Promise<void> {
    const response = await firstValueFrom(
      this.http.post<LoginResponse>('/api/v1/auth/login', { email, password }),
    );

    if (response.user.role !== 'Admin') {
      throw new Error('Admin access is required. Sign in with an administrator account.');
    }

    sessionStorage.setItem(this.tokenKey, response.accessToken);
    sessionStorage.setItem(this.roleKey, response.user.role);
    sessionStorage.setItem(this.nameKey, response.user.fullName);
  }

  signOut(): void {
    sessionStorage.removeItem(this.tokenKey);
    sessionStorage.removeItem(this.roleKey);
    sessionStorage.removeItem(this.nameKey);
  }

  private getRole(): string | null {
    return sessionStorage.getItem(this.roleKey);
  }
}
