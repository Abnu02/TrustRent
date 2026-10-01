import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

export type PropertyReviewStatus = 'Pending' | 'Approved' | 'Rejected' | 'DocumentsRequested';

export interface PropertyReview {
  id: string;
  ownerUserId: string;
  ownerName: string;
  ownerEmail: string;
  address: string;
  city: string;
  state: string;
  postalCode: string;
  bedrooms: number;
  bathrooms: number;
  squareFeet: number;
  monthlyRent: number;
  description: string;
  deedFileNumber: string;
  recordedOwner: string;
  parcelId: string;
  utilityStatus: string;
  photoUrls: string[];
  reviewStatus: PropertyReviewStatus;
  reviewNote: string | null;
  submittedAt: string;
  reviewedAt: string | null;
}

export interface PropertyReviewSummary {
  pending: number;
  approved: number;
  rejected: number;
  documentsRequested: number;
}

@Injectable({ providedIn: 'root' })
export class PropertyReviewApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/v1/admin/properties';

  getAll(): Promise<PropertyReview[]> {
    return firstValueFrom(this.http.get<PropertyReview[]>(this.baseUrl));
  }

  getById(id: string): Promise<PropertyReview> {
    return firstValueFrom(this.http.get<PropertyReview>(`${this.baseUrl}/${encodeURIComponent(id)}`));
  }

  getSummary(): Promise<PropertyReviewSummary> {
    return firstValueFrom(this.http.get<PropertyReviewSummary>(`${this.baseUrl}/summary`));
  }

  review(id: string, status: Exclude<PropertyReviewStatus, 'Pending'>, reviewNote: string): Promise<PropertyReview> {
    return firstValueFrom(
      this.http.patch<PropertyReview>(`${this.baseUrl}/${encodeURIComponent(id)}/review`, {
        status,
        reviewNote,
      }),
    );
  }
}
