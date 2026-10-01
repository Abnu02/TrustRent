import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
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
  property: PropertyReview | null = null;
  loading = true;
  saving = false;
  errorMessage = '';
  decisionMessage = '';

  async ngOnInit(): Promise<void> {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.errorMessage = 'Property ID is missing.';
      this.loading = false;
      return;
    }

    try {
      this.property = await this.api.getById(id);
    } catch (error) {
      this.errorMessage = this.getErrorMessage(error);
    } finally {
      this.loading = false;
    }
  }

  async decide(status: Exclude<PropertyReviewStatus, 'Pending'>): Promise<void> {
    if (!this.property || this.saving) return;
    this.saving = true;
    this.errorMessage = '';

    try {
      this.property = await this.api.review(this.property.id, status, '');
      this.decisionMessage = `Property ${status.toLocaleLowerCase()}.`;
    } catch (error) {
      this.errorMessage = this.getErrorMessage(error);
    } finally {
      this.saving = false;
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
