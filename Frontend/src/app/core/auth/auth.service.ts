import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import {
  LoginRequest,
  LoginResponse,
  RegisterRequest,
  UserResponse
} from './auth.models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly apiUrl = '/api/v1/auth';

  readonly currentUser = signal<UserResponse | null>(
    this.getStoredUser()
  );

  readonly isAuthenticated = signal<boolean>(
    !!localStorage.getItem('trustRent_access_token')
  );

  login(request: LoginRequest) {
    return this.http.post<LoginResponse>(
      `${this.apiUrl}/login`,
      request
    );
  }

  register(request: RegisterRequest) {
    return this.http.post<UserResponse>(
      `${this.apiUrl}/register`,
      request
    );
  }

  saveSession(response: LoginResponse): void {
    localStorage.setItem(
      'trustRent_access_token',
      response.accessToken
    );

    const user: UserResponse = {
      id: response.id,
      fullName: response.fullName,
      email: response.email,
      phoneNumber: response.phoneNumber,
      role: response.role,
      isVerified: response.isVerified
    };

    localStorage.setItem(
      'trustRent_user',
      JSON.stringify(user)
    );

    this.currentUser.set(user);
    this.isAuthenticated.set(true);
  }

  logout(): void {
    this.http.post(`${this.apiUrl}/logout`, {}).subscribe({
      next: () => this.clearSession(),
      error: () => this.clearSession()
    });
  }

  clearSession(): void {
    localStorage.removeItem('trustRent_access_token');
    localStorage.removeItem('trustRent_user');

    this.currentUser.set(null);
    this.isAuthenticated.set(false);

    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return localStorage.getItem('trustRent_access_token');
  }

  private getStoredUser(): UserResponse | null {
    const user = localStorage.getItem('trustRent_user');

    if (!user) {
      return null;
    }

    try {
      return JSON.parse(user) as UserResponse;
    } catch {
      return null;
    }
  }
}