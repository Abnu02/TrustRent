import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

import { LandlordPropertyService } from '../../services/landlord-property.service';
import { LandlordProperty } from '../../models/landlord-property.model';

@Component({
  selector: 'app-landlord-dashboard',
  standalone: true,
  imports: [RouterLink, DecimalPipe],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  private readonly propertyService = inject(LandlordPropertyService);

  readonly properties = signal<LandlordProperty[]>([]);
  readonly loading = signal(true);
  readonly errorMessage = signal('');

  readonly totalProperties = computed(() => this.properties().length);
  readonly pendingProperties = computed(
    () => this.properties().filter(property => property.status === 'Pending').length
  );
  readonly approvedProperties = computed(
    () => this.properties().filter(property => property.status === 'Approved').length
  );
  readonly rejectedProperties = computed(
    () => this.properties().filter(property => property.status === 'Rejected').length
  );
  readonly recentProperties = computed(() => this.properties().slice(0, 3));

  ngOnInit(): void {
    this.loadProperties();
  }

  loadProperties(): void {
    this.loading.set(true);
    this.errorMessage.set('');

    this.propertyService.getMyProperties().subscribe({
      next: properties => {
        this.properties.set(properties);
        this.loading.set(false);
      },
      error: error => {
        this.loading.set(false);
        this.errorMessage.set(this.getErrorMessage(error));
      }
    });
  }

  getStatusClass(status: string): string {
    switch (status) {
      case 'Approved':
        return 'status-approved';
      case 'Rejected':
        return 'status-rejected';
      case 'Archived':
        return 'status-archived';
      default:
        return 'status-pending';
    }
  }

  private getErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      if (error.status === 0) {
        return 'Cannot connect to the server. Check that the API is running.';
      }
      if (error.status === 401 || error.status === 403) {
        return 'Your landlord session may have expired. Sign in again.';
      }
      return error.error?.message ??
        `Unable to load your properties (HTTP ${error.status}).`;
    }

    return 'Unable to load your properties.';
  }
}
