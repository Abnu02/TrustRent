import { Injectable, signal, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { AuthResponse, UserProfile } from '../models/property.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5151/api/v1/auth';

  // Seeded Landlord as initial default for instant hackathon testing
  currentUser = signal<UserProfile>({
    id: '8c1d2e3f-4a5b-6c7d-8e9f-0a1b2c3d4e5f',
    fullName: 'Abreham Bekele',
    email: 'abreham@example.com',
    phoneNumber: '0912345678',
    role: 'Landlord',
    isVerified: true
  });

  registerLandlord(fullName: string, email: string, phoneNumber: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, {
      fullName,
      email,
      phoneNumber,
      password,
      role: 'Landlord'
    }).pipe(
      tap(res => {
        this.currentUser.set(res.user);
      })
    );
  }

  login(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, {
      email,
      password
    }).pipe(
      tap(res => {
        this.currentUser.set(res.user);
      })
    );
  }

  logout() {
    this.currentUser.set({
      id: '',
      fullName: 'Guest',
      email: '',
      role: 'Landlord'
    });
  }
}
