import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
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
  readonly submissions = signal<LandlordReview[]>([]);
  readonly loading = signal(true);
  readonly savingId = signal<string | null>(null);
  readonly rejectionId = signal<string | null>(null);
  readonly rejectionReason = signal('');
  readonly errorMessage = signal('');
  readonly successMessage = signal('');
  readonly searchQuery = signal('');

  async ngOnInit(): Promise<void> {
    await this.load();
  }

  async verify(submission: LandlordReview): Promise<void> {
    this.savingId.set(submission.id);
    this.errorMessage.set('');
    this.successMessage.set('');
    try {
      await this.api.verify(submission.id);
      this.removeFromQueue(submission.id);
      this.successMessage.set(`${submission.fullName} was verified.`);
    } catch (error) {
      this.errorMessage.set(this.getErrorMessage(error));
    } finally {
      this.savingId.set(null);
    }
  }

  beginReject(submission: LandlordReview): void {
    this.rejectionId.set(submission.id);
    this.rejectionReason.set('');
    this.errorMessage.set('');
    this.successMessage.set('');
  }

  cancelReject(): void {
    this.rejectionId.set(null);
    this.rejectionReason.set('');
  }

  async reject(submission: LandlordReview): Promise<void> {
    this.savingId.set(submission.id);
    this.errorMessage.set('');
    this.successMessage.set('');
    try {
      await this.api.reject(submission.id, this.rejectionReason().trim());
      this.removeFromQueue(submission.id);
      this.successMessage.set(`${submission.fullName} was rejected.`);
      this.cancelReject();
    } catch (error) {
      this.errorMessage.set(this.getErrorMessage(error));
    } finally {
      this.savingId.set(null);
    }
  }

  get filteredSubmissions(): LandlordReview[] {
    const query = this.searchQuery().trim().toLocaleLowerCase();
    return this.submissions().filter((submission) =>
      `${submission.fullName} ${submission.email} ${submission.phoneNumber}`.toLocaleLowerCase().includes(query),
    );
  }

  async load(): Promise<void> {
    this.loading.set(true);
    this.errorMessage.set('');
    try {
      this.submissions.set(await this.api.getPending());
    } catch (error) {
      this.errorMessage.set(this.getErrorMessage(error));
    } finally {
      this.loading.set(false);
    }
  }

  private removeFromQueue(id: string): void {
    this.submissions.update(submissions => submissions.filter(submission => submission.id !== id));
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
      this.searchQuery.set(event.target.value);
    }
  }
}
