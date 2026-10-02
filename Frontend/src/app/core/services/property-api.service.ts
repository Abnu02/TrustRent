import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import {
  Property,
  PropertyDetails
} from '../models/property.model';

@Injectable({
  providedIn: 'root'
})
export class PropertyApiService {

  private readonly http = inject(HttpClient);

  private readonly baseUrl = '/api/v1/properties';

  getProperties(filters?: {
    location?: string;
    minRent?: number;
    maxRent?: number;
    bedrooms?: number;
  }): Observable<Property[]> {

    let params = new HttpParams();

    if (filters?.location) {
      params = params.set('location', filters.location);
    }

    if (filters?.minRent !== undefined) {
      params = params.set('minRent', filters.minRent);
    }

    if (filters?.maxRent !== undefined) {
      params = params.set('maxRent', filters.maxRent);
    }

    if (filters?.bedrooms !== undefined) {
      params = params.set('bedrooms', filters.bedrooms);
    }

    return this.http.get<Property[]>(
      this.baseUrl,
      { params }
    );
  }

  getPropertyById(id: string): Observable<PropertyDetails> {
    return this.http.get<PropertyDetails>(
      `${this.baseUrl}/${id}`
    );
  }
}