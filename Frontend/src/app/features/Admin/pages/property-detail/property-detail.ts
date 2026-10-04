import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AdminIcon } from '../../components/admin-icon/admin-icon';
import { PropertyReview, PropertyReviewApi, PropertyReviewStatus } from '../../services/property-review-api';

@Component({
  selector: 'app-property-detail',
  standalone: true,
  imports: [AdminIcon, DatePipe, DecimalPipe, RouterLink],
  templateUrl: './property-detail.html',
  styleUrl: './property-detail.scss',
})
export class PropertyDetail implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(PropertyReviewApi);
  readonly property = signal<PropertyReview | null>(null);
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly savingStatus = signal<Exclude<PropertyReviewStatus, 'Pending'> | null>(null);
  readonly errorMessage = signal('');
  readonly decisionMessage = signal('');
  readonly reviewNote = signal('');

  async ngOnInit(): Promise<void> {
    await this.load();
  }

  async load(): Promise<void> {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.errorMessage.set('Property ID is missing.');
      this.loading.set(false);
      return;
    }

    this.loading.set(true);
    this.errorMessage.set('');
    try {
      this.property.set(await this.api.getById(id));
    } catch (error) {
      this.errorMessage.set(this.getErrorMessage(error));
    } finally {
      this.loading.set(false);
    }
  }

  async decide(status: Exclude<PropertyReviewStatus, 'Pending'>): Promise<void> {
    const property = this.property();
    if (!property || this.saving()) return;
    this.saving.set(true);
    this.savingStatus.set(status);
    this.errorMessage.set('');

    try {
      this.property.set(await this.api.review(property.id, status, this.reviewNote().trim()));
      this.decisionMessage.set(`Property ${status === 'Approved' ? 'verified and approved' : status === 'Rejected' ? 'rejected' : 'marked for document review'}.`);
    } catch (error) {
      this.errorMessage.set(this.getErrorMessage(error));
    } finally {
      this.saving.set(false);
      this.savingStatus.set(null);
    }
  }

  setReviewNote(event: Event): void {
    if (event.target instanceof HTMLTextAreaElement) {
      this.reviewNote.set(event.target.value);
    }
  }

  private getErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse && error.status === 0) {
      return 'Could not connect to the TrustRent API. Check that the backend is running.';
    }
    if (error instanceof HttpErrorResponse && error.status === 401) return 'Your session expired. Sign in again.';
    if (error instanceof HttpErrorResponse && error.status === 403) return 'Admin access is required.';
    if (error instanceof HttpErrorResponse && error.status === 404) return 'This property could not be found.';
    return 'The property request could not be completed.';
  }
}
