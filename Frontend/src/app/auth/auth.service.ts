import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';

export interface AuthUser {
  id: string;
  fullName: string;
  email: string;
  role: string;
  isVerified: boolean;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  fullName: string;
  email: string;
  phoneNumber: string;
  password: string;
  role: 'Landlord';
}

export interface AuthResponse {
  accessToken: string;
  user: AuthUser;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly tokenKey = 'trustrent.accessToken';
  private readonly userKey = 'trustrent.authUser';
  private readonly tokenState = signal(this.readToken());
  readonly user = signal(this.readUser());

  get accessToken(): string | null {
    const token = this.tokenState();
    return token && this.isTokenValid(token) ? token : null;
  }

  get isAuthenticated(): boolean {
    return this.accessToken !== null && this.user() !== null;
  }

  hasRole(role: string): boolean {
    return this.isAuthenticated && this.user()?.role.toLowerCase() === role.toLowerCase();
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>('/api/v1/auth/login', request).pipe(
      tap(response => this.saveSession(response)),
    );
  }

  register(request: RegisterRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>('/api/v1/auth/register', request);
  }

  logout(): Observable<{ message: string }> {
    return this.http.post<{ message: string }>('/api/v1/auth/logout', {});
  }

  clearSession(): void {
    sessionStorage.removeItem(this.tokenKey);
    sessionStorage.removeItem(this.userKey);
    this.tokenState.set(null);
    this.user.set(null);
  }

  private saveSession(response: AuthResponse): void {
    sessionStorage.setItem(this.tokenKey, response.accessToken);
    sessionStorage.setItem(this.userKey, JSON.stringify(response.user));
    this.tokenState.set(response.accessToken);
    this.user.set(response.user);
  }

  private readToken(): string | null {
    return sessionStorage.getItem(this.tokenKey);
  }

  private readUser(): AuthUser | null {
    try {
      const value = sessionStorage.getItem(this.userKey);
      if (!value) return null;
      const user: unknown = JSON.parse(value);
      if (!isAuthUser(user)) return null;
      return user;
    } catch {
      return null;
    }
  }

  private isTokenValid(token: string): boolean {
    try {
      const payload = token.split('.')[1];
      if (!payload) return false;
      const normalized = payload.replace(/-/g, '+').replace(/_/g, '/');
      const claims: unknown = JSON.parse(atob(normalized));
      if (typeof claims !== 'object' || claims === null || !('exp' in claims)) return false;
      const expiresAt = Number((claims as { exp: unknown }).exp) * 1000;
      return Number.isFinite(expiresAt) && expiresAt > Date.now();
    } catch {
      return false;
    }
  }
}

function isAuthUser(value: unknown): value is AuthUser {
  if (typeof value !== 'object' || value === null) return false;
  const user = value as Record<string, unknown>;
  return typeof user['id'] === 'string'
    && typeof user['fullName'] === 'string'
    && typeof user['email'] === 'string'
    && typeof user['role'] === 'string'
    && typeof user['isVerified'] === 'boolean';
}