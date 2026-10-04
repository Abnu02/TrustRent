import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AdminIcon } from '../../components/admin-icon/admin-icon';
import { PropertyReview, PropertyReviewApi } from '../../services/property-review-api';

@Component({
  selector: 'app-admin-overview',
  standalone: true,
  imports: [RouterLink, AdminIcon, DatePipe],
  templateUrl: './overview.html',
  styleUrl: './overview.scss',
})
export class AdminOverview {
  readonly metrics = [
    { label: 'Pending reviews', icon: 'clock' as const, key: 'pending' as const },
    { label: 'Approved properties', icon: 'property' as const, key: 'approved' as const },
    { label: 'Rejected properties', icon: 'audit' as const, key: 'rejected' as const },
    { label: 'Documents requested', icon: 'document' as const, key: 'documentsRequested' as const },
  ];

  summary = { pending: 0, approved: 0, rejected: 0, documentsRequested: 0 };
  queue: PropertyReview[] = [];
  loading = true;
  errorMessage = '';
  private readonly api = inject(PropertyReviewApi);

  constructor() {
    void this.load();
  }

  private async load(): Promise<void> {
    try {
      [this.queue, this.summary] = await Promise.all([this.api.getAll(), this.api.getSummary()]);
      this.queue = this.queue.slice(0, 5);
    } catch (error) {
      this.errorMessage = error instanceof HttpErrorResponse && error.status === 0
        ? 'Could not connect to the TrustRent API.'
        : error instanceof HttpErrorResponse && error.status === 403
          ? 'Admin access is required to view dashboard data.'
          : 'Dashboard data could not be loaded.';
    } finally {
      this.loading = false;
    }
  }
}
