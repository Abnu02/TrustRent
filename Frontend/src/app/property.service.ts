import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { EMPTY, Observable, expand, forkJoin, map, reduce } from 'rxjs';

export type PropertyStatus = 'Pending' | 'Approved' | 'Rejected';
export type PropertyType = 'Apartment' | 'House' | 'Studio';

export interface PropertyDraft {
  title: string;
  description: string;
  propertyType: PropertyType;
  rent: number;
  deposit: number;
  location: string;
  bedrooms: number;
  bathrooms: number;
}

export interface Property extends PropertyDraft {
  id: string;
  status: PropertyStatus;
  isVerified: boolean;
}

export interface PagedProperties {
  items: Property[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface PropertyQuery {
  page?: number;
  pageSize?: number;
  status?: PropertyStatus;
  propertyType?: PropertyType;
  search?: string;
  sortBy?: 'rent' | 'title' | 'location';
  sortDirection?: 'asc' | 'desc';
}

export interface DashboardSnapshot {
  total: number;
  pending: number;
  approved: number;
  rejected: number;
  verified: number;
  recentProperties: Property[];
}

@Injectable({ providedIn: 'root' })
export class PropertyService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = '/api/v1/properties';

  getMy(query: PropertyQuery): Observable<PagedProperties> {
    let params = new HttpParams();
    if (query.page !== undefined) params = params.set('page', query.page);
    if (query.pageSize !== undefined) params = params.set('pageSize', query.pageSize);
    if (query.status) params = params.set('status', query.status);
    if (query.propertyType) params = params.set('propertyType', query.propertyType);
    if (query.search) params = params.set('search', query.search);
    if (query.sortBy) params = params.set('sortBy', query.sortBy);
    if (query.sortDirection) params = params.set('sortDirection', query.sortDirection);
    return this.http.get<PagedProperties>(`${this.endpoint}/my`, { params });
  }

  getDashboardSnapshot(): Observable<DashboardSnapshot> {
    return forkJoin({
      all: this.getAllMyProperties(),
      pending: this.getMy({ page: 1, pageSize: 1, status: 'Pending' }),
      approved: this.getMy({ page: 1, pageSize: 1, status: 'Approved' }),
      rejected: this.getMy({ page: 1, pageSize: 1, status: 'Rejected' }),
      recent: this.getMy({ page: 1, pageSize: 5, sortBy: 'title', sortDirection: 'asc' }),
    }).pipe(map(({ all, pending, approved, rejected, recent }) => ({
      total: all.length,
      pending: pending.totalCount,
      approved: approved.totalCount,
      rejected: rejected.totalCount,
      verified: all.filter(property => property.isVerified).length,
      recentProperties: recent.items,
    })));
  }

  getById(id: string): Observable<Property> {
    return this.http.get<Property>(`${this.endpoint}/${id}`);
  }

  create(draft: PropertyDraft): Observable<Property> {
    return this.http.post<Property>(this.endpoint, draft);
  }

  update(id: string, draft: PropertyDraft): Observable<Property> {
    return this.http.put<Property>(`${this.endpoint}/${id}`, draft);
  }

  private getAllMyProperties(): Observable<Property[]> {
    return this.getMy({ page: 1, pageSize: 50 }).pipe(
      expand(result => result.page < result.totalPages
        ? this.getMy({ page: result.page + 1, pageSize: 50 })
        : EMPTY),
      reduce((all, result) => all.concat(result.items), [] as Property[]),
    );
  }
}