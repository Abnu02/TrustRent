import { HttpErrorResponse } from '@angular/common/http';
import {
  ChangeDetectorRef,
  Component,
  inject,
  OnInit
} from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { AdminService } from '../services/admin.service';
import { AdminProperty } from '../models/admin.models';

@Component({
  selector: 'app-pending-properties',
  standalone: true,
  imports: [CommonModule, DecimalPipe, FormsModule],
  templateUrl: './pending-properties.html',
  styleUrl: './pending-properties.scss'
})
export class PendingPropertiesComponent implements OnInit {
  private readonly adminService = inject(AdminService);
  private readonly changeDetector = inject(ChangeDetectorRef);

  properties: AdminProperty[] = [];
  loading = true;
  processingId: string | null = null;
  errorMessage = '';
  successMessage = '';
  selectedProperty: AdminProperty | null = null;
  showDetails = false;
  showRejectDialog = false;
  rejectionReason = '';

  get pendingCount(): number {
    return this.properties.filter(property => property.status === 'Pending').length;
  }

  ngOnInit(): void {
    this.loadProperties();
  }

  loadProperties(): void {
    this.loading = true;
    this.errorMessage = '';

    this.adminService.getAllProperties().subscribe({
      next: properties => {
        this.properties = properties;
        this.loading = false;
        this.changeDetector.markForCheck();
      },
      error: error => {
        this.loading = false;
        this.errorMessage = this.getErrorMessage(
          error,
          'Unable to load properties.'
        );
        this.changeDetector.markForCheck();
      }
    });
  }

  viewDetails(property: AdminProperty): void {
    this.selectedProperty = property;
    this.showDetails = true;
  }

  closeDetails(): void {
    this.showDetails = false;
    this.selectedProperty = null;
  }

  approve(property: AdminProperty): void {
    this.processingId = property.id;
    this.errorMessage = '';

    this.adminService.approveProperty(property.id).subscribe({
      next: () => {
        this.updateProperty(property.id, {
          ...property,
          status: 'Approved',
          isVerified: true
        });
        this.processingId = null;
        this.successMessage = `${property.title} has been approved.`;
        this.changeDetector.markForCheck();
      },
      error: error => {
        this.processingId = null;
        this.errorMessage = this.getErrorMessage(
          error,
          'Unable to approve property.'
        );
        this.changeDetector.markForCheck();
      }
    });
  }

  openReject(property: AdminProperty): void {
    this.selectedProperty = property;
    this.rejectionReason = '';
    this.showRejectDialog = true;
  }

  closeReject(): void {
    this.showRejectDialog = false;
    this.selectedProperty = null;
    this.rejectionReason = '';
  }

  reject(): void {
    const property = this.selectedProperty;
    const reason = this.rejectionReason.trim();

    if (!property || !reason) {
      return;
    }

    this.processingId = property.id;
    this.errorMessage = '';

    this.adminService.rejectProperty(property.id, { reason }).subscribe({
      next: () => {
        this.updateProperty(property.id, {
          ...property,
          status: 'Rejected',
          isVerified: false
        });
        this.processingId = null;
        this.successMessage = `${property.title} has been rejected.`;
        this.closeReject();
        this.changeDetector.markForCheck();
      },
      error: error => {
        this.processingId = null;
        this.errorMessage = this.getErrorMessage(
          error,
          'Unable to reject property.'
        );
        this.changeDetector.markForCheck();
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

  private updateProperty(id: string, updated: AdminProperty): void {
    this.properties = this.properties.map(property =>
      property.id === id ? updated : property
    );

    if (this.selectedProperty?.id === id) {
      this.selectedProperty = updated;
    }
  }

  private getErrorMessage(error: unknown, fallback: string): string {
    if (error instanceof HttpErrorResponse) {
      if (error.status === 0) {
        return 'Cannot connect to the API. Check that the backend is running.';
      }
      if (error.status === 401 || error.status === 403) {
        return 'Your admin session is invalid or expired. Sign in again.';
      }
      return error.error?.message ?? `${fallback} (HTTP ${error.status}).`;
    }

    return fallback;
  }
}
