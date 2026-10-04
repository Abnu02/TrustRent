import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TimeoutError } from 'rxjs';

import { AdminService } from '../services/admin.service';
import {
  CreateLandlordRequest,
  AdminLandlord,
  UpdateLandlordRequest
} from '../models/admin.models';

@Component({
  selector: 'app-landlord-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './pending-landlords.html',
  styleUrl: './pending-landlords.scss'
})
export class LandlordManagementComponent implements OnInit {
  private readonly adminService = inject(AdminService);
  private readonly changeDetector = inject(ChangeDetectorRef);

  landlords: AdminLandlord[] = [];
  loading = true;
  saving = false;
  processingId: string | null = null;
  errorMessage = '';
  successMessage = '';
  editorMode: 'create' | 'edit' | null = null;
  selectedLandlord: AdminLandlord | null = null;

  formFullName = '';
  formEmail = '';
  formPhoneNumber = '';
  formPassword = '';

  get pendingCount(): number {
    return this.landlords.filter(landlord => !landlord.isVerified).length;
  }

  ngOnInit(): void {
    this.loadLandlords();
  }

  loadLandlords(): void {
    this.loading = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.adminService.getLandlords().subscribe({
      next: landlords => {
        this.landlords = landlords;
        this.loading = false;
        this.changeDetector.markForCheck();
      },
      error: error => {
        this.loading = false;
        this.errorMessage = this.getErrorMessage(
          error,
          'Unable to load landlord accounts.'
        );
        this.changeDetector.markForCheck();
      }
    });
  }

  openCreate(): void {
    this.selectedLandlord = null;
    this.formFullName = '';
    this.formEmail = '';
    this.formPhoneNumber = '';
    this.formPassword = '';
    this.editorMode = 'create';
    this.errorMessage = '';
  }

  openEdit(landlord: AdminLandlord): void {
    this.selectedLandlord = landlord;
    this.formFullName = landlord.fullName;
    this.formEmail = landlord.email;
    this.formPhoneNumber = landlord.phoneNumber;
    this.editorMode = 'edit';
    this.errorMessage = '';
  }

  closeEditor(): void {
    if (this.saving) {
      return;
    }

    this.editorMode = null;
    this.selectedLandlord = null;
    this.errorMessage = '';
  }

  saveLandlord(): void {
    const fullName = this.formFullName.trim();
    const email = this.formEmail.trim();
    const phoneNumber = this.formPhoneNumber.trim();
    const password = this.formPassword;

    if (!fullName || !email) {
      this.errorMessage = 'Name and email are required.';
      return;
    }

    if (this.editorMode === 'create' && !password) {
      this.errorMessage = 'A password is required for a new account.';
      return;
    }

    this.saving = true;
    this.errorMessage = '';

    if (this.editorMode === 'create') {
      const request: CreateLandlordRequest = {
        fullName,
        email,
        phoneNumber,
        password
      };

      this.adminService.createLandlord(request).subscribe({
        next: landlord => {
          this.landlords = [...this.landlords, landlord]
            .sort((left, right) => left.fullName.localeCompare(right.fullName));
          this.saving = false;
          this.editorMode = null;
          this.successMessage = `${landlord.fullName} was added.`;
          this.changeDetector.markForCheck();
        },
        error: error => {
          this.saving = false;
          this.errorMessage = this.getErrorMessage(
            error,
            'Unable to create the landlord account.'
          );
          this.changeDetector.markForCheck();
        }
      });
      return;
    }

    const landlord = this.selectedLandlord;
    if (!landlord) {
      this.saving = false;
      this.errorMessage = 'Select a landlord account to edit.';
      return;
    }

    const request: UpdateLandlordRequest = {
      fullName,
      email,
      phoneNumber
    };

    this.adminService.updateLandlord(landlord.id, request).subscribe({
      next: updatedLandlord => {
        this.replaceLandlord(updatedLandlord);
        this.saving = false;
        this.editorMode = null;
        this.selectedLandlord = null;
        this.successMessage = `${updatedLandlord.fullName} was updated.`;
        this.changeDetector.markForCheck();
      },
      error: error => {
        this.saving = false;
        this.errorMessage = this.getErrorMessage(
          error,
          'Unable to update the landlord account.'
        );
        this.changeDetector.markForCheck();
      }
    });
  }

  setVerification(landlord: AdminLandlord): void {
    this.processingId = landlord.id;
    this.errorMessage = '';

    const request = landlord.isVerified
      ? this.adminService.rejectLandlord(
          landlord.id,
          { reason: 'Approval revoked by administrator.' }
        )
      : this.adminService.verifyLandlord(landlord.id);

    request.subscribe({
      next: () => {
        this.replaceLandlord({
          ...landlord,
          isVerified: !landlord.isVerified
        });
        this.processingId = null;
        this.successMessage = landlord.isVerified
          ? `${landlord.fullName}'s approval was revoked.`
          : `${landlord.fullName} was approved.`;
        this.changeDetector.markForCheck();
      },
      error: error => {
        this.processingId = null;
        this.errorMessage = this.getErrorMessage(
          error,
          'Unable to update landlord approval.'
        );
        this.changeDetector.markForCheck();
      }
    });
  }

  toggleActive(landlord: AdminLandlord): void {
    const activating = !landlord.isActive;

    if (!activating &&
        !confirm(`Deactivate ${landlord.fullName}'s account? Their properties will be preserved.`)) {
      return;
    }

    this.processingId = landlord.id;
    this.errorMessage = '';

    const request = activating
      ? this.adminService.setLandlordActive(landlord.id, true)
      : this.adminService.deactivateLandlord(landlord.id);

    request.subscribe({
      next: () => {
        this.replaceLandlord({ ...landlord, isActive: activating });
        this.processingId = null;
        this.successMessage = activating
          ? `${landlord.fullName}'s account was reactivated.`
          : `${landlord.fullName}'s account was deactivated.`;
        this.changeDetector.markForCheck();
      },
      error: error => {
        this.processingId = null;
        this.errorMessage = this.getErrorMessage(
          error,
          'Unable to change landlord account status.'
        );
        this.changeDetector.markForCheck();
      }
    });
  }

  private replaceLandlord(updatedLandlord: AdminLandlord): void {
    this.landlords = this.landlords
      .map(landlord =>
        landlord.id === updatedLandlord.id ? updatedLandlord : landlord
      )
      .sort((left, right) => left.fullName.localeCompare(right.fullName));
  }

  private getErrorMessage(error: unknown, fallback: string): string {
    if (error instanceof TimeoutError) {
      return 'The API did not respond in time. Check that the backend is running.';
    }

    if (error instanceof HttpErrorResponse) {
      if (error.status === 0) {
        return 'Cannot reach the API. Check that the backend is running.';
      }

      if (error.status === 401 || error.status === 403) {
        return 'Your admin session is invalid or expired. Sign in again.';
      }

      return error.error?.message ?? `${fallback} (HTTP ${error.status}).`;
    }

    return fallback;
  }
}
