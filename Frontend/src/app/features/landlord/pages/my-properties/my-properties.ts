import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';

import { LandlordProperty } from '../../models/landlord-property.model';
import { LandlordPropertyService } from '../../services/landlord-property.service';

@Component({
  selector: 'app-my-properties',
  standalone: true,
  imports: [DecimalPipe, RouterLink],
  templateUrl: './my-properties.html',
  styleUrl: './my-properties.scss'
})
export class MyPropertiesComponent implements OnInit {
  private readonly propertyService = inject(LandlordPropertyService);

  readonly properties = signal<LandlordProperty[]>([]);
  readonly loading = signal(true);
  readonly errorMessage = signal('');
  readonly archivingId = signal<string | null>(null);
  readonly successMessage = signal('');

  ngOnInit(): void {
    this.loadProperties();
  }

  loadProperties(): void {
    this.loading.set(true);
    this.errorMessage.set('');
    this.successMessage.set('');

    this.propertyService.getMyProperties().subscribe({
      next: properties => {
        this.properties.set(properties);
        this.loading.set(false);
      },
      error: error => {
        this.loading.set(false);
        this.errorMessage.set(
          this.getErrorMessage(error, 'Unable to load your properties.')
        );
      }
    });
  }

  archiveProperty(property: LandlordProperty): void {
    const confirmed = globalThis.confirm(
      `Archive "${property.title}"? It will no longer appear to tenants, but its record will remain in your account.`
    );

    if (!confirmed) {
      return;
    }

    this.archivingId.set(property.id);
    this.errorMessage.set('');
    this.successMessage.set('');

    this.propertyService.archiveProperty(property.id).subscribe({
      next: () => {
        this.properties.update(properties =>
          properties.map(item =>
            item.id === property.id
              ? { ...item, status: 'Archived', isVerified: false }
              : item
          )
        );
        this.archivingId.set(null);
        this.successMessage.set(
          `"${property.title}" has been archived and is no longer visible to tenants.`
        );
      },
      error: error => {
        this.archivingId.set(null);
        this.errorMessage.set(
          this.getErrorMessage(error, 'Unable to archive this property.')
        );
      }
    });
  }

  statusClass(status: string): string {
    switch (status) {
      case 'Approved':
        return 'approved';
      case 'Rejected':
        return 'rejected';
      case 'Archived':
        return 'archived';
      default:
        return 'pending';
    }
  }

  private getErrorMessage(error: unknown, fallback: string): string {
    if (error instanceof HttpErrorResponse) {
      if (error.status === 0) {
        return 'Cannot connect to the server. Check that the API is running.';
      }
      if (error.status === 401 || error.status === 403) {
        return 'Your landlord session may have expired. Sign in again.';
      }
      if (error.status === 404) {
        return 'This property could not be found.';
      }
      return error.error?.message ??
        `${fallback} (HTTP ${error.status}).`;
    }

    return fallback;
  }
}
