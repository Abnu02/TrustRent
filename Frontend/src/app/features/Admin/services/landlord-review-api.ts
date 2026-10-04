import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

export type LandlordVerificationStatus = 'Pending' | 'Verified' | 'Rejected';

export interface LandlordReview {
  id: string;
  fullName: string;
  email: string;
  phoneNumber: string;
  verificationStatus: LandlordVerificationStatus;
  createdAt: string;
  reviewedAt: string | null;
  reviewNote: string | null;
}

@Injectable({ providedIn: 'root' })
export class LandlordReviewApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/v1/admin/landlords';

  getPending(): Promise<LandlordReview[]> {
    return firstValueFrom(this.http.get<LandlordReview[]>(`${this.baseUrl}/pending`));
  }

  verify(id: string): Promise<LandlordReview> {
    return firstValueFrom(
      this.http.put<LandlordReview>(`${this.baseUrl}/${encodeURIComponent(id)}/verify`, {}),
    );
  }

  reject(id: string, reason: string): Promise<LandlordReview> {
    return firstValueFrom(
      this.http.put<LandlordReview>(`${this.baseUrl}/${encodeURIComponent(id)}/reject`, { reason }),
    );
  }
}
