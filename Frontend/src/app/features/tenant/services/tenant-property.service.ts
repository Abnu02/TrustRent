import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';

import {
  Property,
  PropertyDetails
} from '../../../core/models/property.model';

@Injectable({
  providedIn: 'root'
})
export class TenantPropertyService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/v1/properties';

  getVerifiedProperties() {
    return this.http.get<Property[]>(this.apiUrl);
  }

  getVerifiedProperty(id: string) {
    return this.http.get<PropertyDetails>(`${this.apiUrl}/${id}`);
  }
}
