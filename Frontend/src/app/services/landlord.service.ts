import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { 
  CreatePropertyDto, 
  MyProperty, 
  PagedResponse, 
  LandlordStats, 
  PropertyCreatedResponse, 
  RegisterLandlordDto 
} from '../models/property.model';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class LandlordService {
  private http = inject(HttpClient);
  private authService = inject(AuthService);
  private apiUrl = 'http://localhost:5151/api/v1/properties';

  private getHeaders(): HttpHeaders {
    const currentId = this.authService.currentUser().id;
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'X-Landlord-Id': currentId || '8c1d2e3f-4a5b-6c7d-8e9f-0a1b2c3d4e5f'
    });
  }

  registerLandlord(dto: RegisterLandlordDto): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/landlord/register`, dto);
  }

  getMyPropertiesPaged(
    page: number = 1,
    pageSize: number = 6,
    status?: string,
    search?: string,
    sortBy: string = 'createdAt',
    sortDescending: boolean = true
  ): Observable<PagedResponse<MyProperty>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString())
      .set('sortBy', sortBy)
      .set('sortDescending', sortDescending.toString());

    if (status && status !== 'All') {
      params = params.set('status', status);
    }

    if (search && search.trim()) {
      params = params.set('search', search.trim());
    }

    return this.http.get<PagedResponse<MyProperty>>(`${this.apiUrl}/my`, {
      headers: this.getHeaders(),
      params
    });
  }

  getLandlordStats(): Observable<LandlordStats> {
    return this.http.get<LandlordStats>(`${this.apiUrl}/my/stats`, {
      headers: this.getHeaders()
    });
  }

  createProperty(data: CreatePropertyDto): Observable<PropertyCreatedResponse> {
    return this.http.post<PropertyCreatedResponse>(this.apiUrl, data, {
      headers: this.getHeaders()
    });
  }

  getPropertyById(id: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${id}`);
  }
}
