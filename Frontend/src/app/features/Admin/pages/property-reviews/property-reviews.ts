import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AdminIcon } from '../../components/admin-icon/admin-icon';
import { PropertyReview, PropertyReviewApi } from '../../services/property-review-api';

@Component({
  selector: 'app-property-reviews',
  standalone: true,
  imports: [AdminIcon, DatePipe, DecimalPipe, RouterLink],
  templateUrl: './property-reviews.html',
  styleUrl: './property-reviews.scss',
})
export class PropertyReviews implements OnInit {
  private readonly api = inject(PropertyReviewApi);
  readonly properties = signal<PropertyReview[]>([]);
  readonly loading = signal(true);
  readonly errorMessage = signal('');
  readonly searchQuery = signal('');
  readonly statusFilter = signal('All');
  readonly filteredProperties = computed(() => {
    const query = this.searchQuery().trim().toLocaleLowerCase();
    const status = this.statusFilter();
    return this.properties().filter((property) => {
      const matchesQuery = `${property.address} ${property.city} ${property.ownerName}`
        .toLocaleLowerCase()
        .includes(query);
      return matchesQuery && (status === 'All' || property.reviewStatus === status);
    });
  });

  async ngOnInit(): Promise<void> {
    await this.load();
  }

  async load(): Promise<void> {
    this.loading.set(true);
    this.errorMessage.set('');
    try {
      this.properties.set(await this.api.getAll());
    } catch (error) {
      this.errorMessage.set(this.getErrorMessage(error));
    } finally {
      this.loading.set(false);
    }
  }

  setSearchQuery(event: Event): void {
    if (event.target instanceof HTMLInputElement) this.searchQuery.set(event.target.value);
  }

  setStatusFilter(event: Event): void {
    if (event.target instanceof HTMLSelectElement) this.statusFilter.set(event.target.value);
  }

  private getErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse && error.status === 0) {
      return 'Could not connect to the TrustRent API. Check that the backend is running.';
    }
    if (error instanceof HttpErrorResponse && error.status === 401) return 'Sign in again to continue.';
    if (error instanceof HttpErrorResponse && error.status === 403) return 'Admin access is required to view property reviews.';
    return 'Property reviews could not be loaded.';
  }
}
