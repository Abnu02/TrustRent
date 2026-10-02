import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import {
  CreatePropertyRequest,
  CreatePropertyResponse,
  LandlordProperty
} from '../models/landlord-property.model';

@Injectable({
  providedIn: 'root'
})
export class LandlordPropertyService {

  private readonly http = inject(HttpClient);

  private readonly apiUrl = '/api/v1/properties';

  /**
   * Create a new property.
   *
   * POST /api/v1/properties
   *
   * landlordId is NOT included.
   * The backend gets it from the authenticated JWT.
   */
  createProperty(
    property: CreatePropertyRequest
  ): Observable<CreatePropertyResponse> {

    return this.http.post<CreatePropertyResponse>(
      this.apiUrl,
      property
    );
  }

  /**
   * Get properties belonging to the
   * currently authenticated landlord.
   *
   * GET /api/v1/properties/my
   */
  getMyProperties(): Observable<LandlordProperty[]> {

    return this.http.get<LandlordProperty[]>(
      `${this.apiUrl}/my`
    );
  }

  /**
   * Update an existing property.
   *
   * PUT /api/v1/properties/{id}
   */
  updateProperty(
    id: string,
    property: CreatePropertyRequest
  ): Observable<void> {

    return this.http.put<void>(
      `${this.apiUrl}/${id}`,
      property
    );
  }
}