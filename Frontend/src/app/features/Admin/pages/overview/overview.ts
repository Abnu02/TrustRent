import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
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
export class AdminOverview implements OnInit {
  readonly metrics = [
    { label: 'Pending reviews', icon: 'clock' as const, key: 'pending' as const },
    { label: 'Approved properties', icon: 'property' as const, key: 'approved' as const },
    { label: 'Rejected properties', icon: 'audit' as const, key: 'rejected' as const },
    { label: 'Documents requested', icon: 'document' as const, key: 'documentsRequested' as const },
  ];

  readonly summary = signal({ pending: 0, approved: 0, rejected: 0, documentsRequested: 0 });
  readonly queue = signal<PropertyReview[]>([]);
  readonly loading = signal(true);
  readonly errorMessage = signal('');
  private readonly api = inject(PropertyReviewApi);

  ngOnInit(): void {
    void this.load();
  }

  async load(): Promise<void> {
    this.loading.set(true);
    this.errorMessage.set('');
    try {
      const [queue, summary] = await Promise.all([this.api.getAll(), this.api.getSummary()]);
      this.queue.set(queue.slice(0, 5));
      this.summary.set(summary);
    } catch (error) {
      this.errorMessage.set(error instanceof HttpErrorResponse && error.status === 0
        ? 'Could not connect to the TrustRent API.'
        : error instanceof HttpErrorResponse && error.status === 403
          ? 'Admin access is required to view dashboard data.'
          : 'Dashboard data could not be loaded.');
    } finally {
      this.loading.set(false);
    }
  }
}
