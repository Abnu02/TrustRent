import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { timeout } from 'rxjs';

import {
  AdminLandlord,
  AdminProperty,
  RejectRequest,
  CreateLandlordRequest,
  UpdateLandlordRequest
} from '../models/admin.models';

@Injectable({
  providedIn: 'root'
})
export class AdminService {

  private readonly http = inject(HttpClient);

  private readonly apiUrl = '/api/v1/admin';


  // =========================
  // LANDLORD VERIFICATION
  // =========================

  getLandlords() {
    return this.http.get<AdminLandlord[]>(
      `${this.apiUrl}/landlords`
    ).pipe(timeout({ first: 15000 }));
  }

  createLandlord(request: CreateLandlordRequest) {
    return this.http.post<AdminLandlord>(
      `${this.apiUrl}/landlords`,
      request
    );
  }

  updateLandlord(id: string, request: UpdateLandlordRequest) {
    return this.http.put<AdminLandlord>(
      `${this.apiUrl}/landlords/${id}`,
      request
    );
  }

  setLandlordActive(id: string, isActive: boolean) {
    return this.http.put(
      `${this.apiUrl}/landlords/${id}/active`,
      { isActive }
    );
  }

  deactivateLandlord(id: string) {
    return this.http.delete(
      `${this.apiUrl}/landlords/${id}`
    );
  }

  verifyLandlord(id: string) {
    return this.http.put(
      `${this.apiUrl}/landlords/${id}/verify`,
      {}
    );
  }

  rejectLandlord(
    id: string,
    request: RejectRequest
  ) {
    return this.http.put(
      `${this.apiUrl}/landlords/${id}/reject`,
      request
    );
  }


  // =========================
  // PROPERTY VERIFICATION
  // =========================

  getAllProperties() {
    return this.http.get<AdminProperty[]>(
      `${this.apiUrl}/properties`
    ).pipe(timeout({ first: 15000 }));
  }

  approveProperty(id: string) {
    return this.http.put(
      `${this.apiUrl}/properties/${id}/approve`,
      {}
    );
  }

  rejectProperty(
    id: string,
    request: RejectRequest
  ) {
    return this.http.put(
      `${this.apiUrl}/properties/${id}/reject`,
      request
    );
  }
}