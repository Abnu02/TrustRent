import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AdminIcon } from '../../components/admin-icon/admin-icon';
import { LandlordReview, LandlordReviewApi } from '../../services/landlord-review-api';

@Component({
  selector: 'app-admin-landlord-reviews',
  standalone: true,
  imports: [AdminIcon, DatePipe, FormsModule],
  templateUrl: './landlord-reviews.html',
  styleUrl: './landlord-reviews.scss',
})
export class LandlordReviews implements OnInit {
  private readonly api = inject(LandlordReviewApi);
  submissions: LandlordReview[] = [];
  loading = true;
  savingId: string | null = null;
  rejectionId: string | null = null;
  rejectionReason = '';
  errorMessage = '';
  successMessage = '';
  searchQuery = '';

  async ngOnInit(): Promise<void> {
    await this.load();
  }

  async verify(submission: LandlordReview): Promise<void> {
    this.savingId = submission.id;
    this.errorMessage = '';
    this.successMessage = '';
    try {
      await this.api.verify(submission.id);
      this.removeFromQueue(submission.id);
      this.successMessage = `${submission.fullName} was verified.`;
    } catch (error) {
      this.errorMessage = this.getErrorMessage(error);
    } finally {
      this.savingId = null;
    }
  }

  beginReject(submission: LandlordReview): void {
    this.rejectionId = submission.id;
    this.rejectionReason = '';
    this.errorMessage = '';
    this.successMessage = '';
  }

  cancelReject(): void {
    this.rejectionId = null;
    this.rejectionReason = '';
  }

  async reject(submission: LandlordReview): Promise<void> {
    this.savingId = submission.id;
    this.errorMessage = '';
    this.successMessage = '';
    try {
      await this.api.reject(submission.id, this.rejectionReason.trim());
      this.removeFromQueue(submission.id);
      this.successMessage = `${submission.fullName} was rejected.`;
      this.cancelReject();
    } catch (error) {
      this.errorMessage = this.getErrorMessage(error);
    } finally {
      this.savingId = null;
    }
  }

  get filteredSubmissions(): LandlordReview[] {
    const query = this.searchQuery.trim().toLocaleLowerCase();
    return this.submissions.filter((submission) =>
      `${submission.fullName} ${submission.email} ${submission.phoneNumber}`.toLocaleLowerCase().includes(query),
    );
  }

  private async load(): Promise<void> {
    this.loading = true;
    this.errorMessage = '';
    try {
      this.submissions = await this.api.getPending();
    } catch (error) {
      this.errorMessage = this.getErrorMessage(error);
    } finally {
      this.loading = false;
    }
  }

  private removeFromQueue(id: string): void {
    this.submissions = this.submissions.filter((submission) => submission.id !== id);
  }

  private getErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse && error.status === 0) {
      return 'Could not connect to the TrustRent API. Check that the backend is running.';
    }
    if (error instanceof HttpErrorResponse && error.status === 401) return 'Your session expired. Sign in again.';
    if (error instanceof HttpErrorResponse && error.status === 403) return 'Admin access is required to review landlords.';
    if (error instanceof HttpErrorResponse && error.status === 404) return 'This landlord could not be found.';
    return 'Landlord review could not be completed.';
  }

  setSearchQuery(event: Event): void {
    if (event.target instanceof HTMLInputElement) {
      this.searchQuery = event.target.value;
    }
  }
}
