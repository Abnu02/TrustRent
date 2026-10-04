import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
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
  properties: PropertyReview[] = [];
  loading = true;
  errorMessage = '';
  searchQuery = '';
  statusFilter = 'All';

  async ngOnInit(): Promise<void> {
    try {
      this.properties = await this.api.getAll();
    } catch (error) {
      this.errorMessage = this.getErrorMessage(error);
    } finally {
      this.loading = false;
    }
  }

  get filteredProperties(): PropertyReview[] {
    const query = this.searchQuery.trim().toLocaleLowerCase();
    return this.properties.filter((property) => {
      const matchesQuery = `${property.address} ${property.city} ${property.ownerName}`
        .toLocaleLowerCase()
        .includes(query);
      return matchesQuery && (this.statusFilter === 'All' || property.reviewStatus === this.statusFilter);
    });
  }

  setSearchQuery(event: Event): void {
    if (event.target instanceof HTMLInputElement) this.searchQuery = event.target.value;
  }

  setStatusFilter(event: Event): void {
    if (event.target instanceof HTMLSelectElement) this.statusFilter = event.target.value;
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
