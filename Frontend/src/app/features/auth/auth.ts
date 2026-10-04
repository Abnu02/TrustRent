import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AdminAuthSession } from '../Admin/services/admin-auth-session';

type AuthMode = 'sign-in' | 'register' | 'reset';
type AuthRole = 'tenant' | 'landlord' | 'admin';

@Component({
  selector: 'app-auth',
  standalone: true,
  imports: [CommonModule, FormsModule],
  styleUrl: './auth.scss',
  templateUrl: './auth.html',
})
export class Auth {
  readonly authModes: ReadonlyArray<{ key: AuthMode; label: string; description: string }> = [
    { key: 'sign-in', label: 'Sign in', description: 'Access your verified tenant dashboard.' },
    { key: 'register', label: 'Create account', description: 'Set up a new TrustRent identity.' },
    { key: 'reset', label: 'Reset password', description: 'Recover a lost or expired passcode.' },
  ];

  readonly roles: ReadonlyArray<{
    key: AuthRole;
    label: string;
    subtitle: string;
    icon: string;
  }> = [
    { key: 'tenant', label: 'Tenant', subtitle: 'Verified rentals', icon: 'person_shield' },
    { key: 'landlord', label: 'Landlord', subtitle: 'Deed-matched units', icon: 'real_estate_agent' },
    { key: 'admin', label: 'Admin', subtitle: 'Property review console', icon: 'policy' },
  ];

  readonly trustHighlights = [
    {
      title: '100% Deed-Authenticated Properties',
      description:
        'Zero phantom listings, algorithmic duplicate sweeps, and guaranteed prevention of bait-and-switch leases.',
      icon: 'domain_verification',
    },
    {
      title: 'Bank-Grade Escrow Protection',
      description:
        'Application deposits, security holds, and first-month disbursements remain locked in FDIC-insured trust vaults.',
      icon: 'account_balance',
    },
    {
      title: 'Auditor Inspection Guarantee',
      description:
        'Automated continuous sync with county CAD deed title filings across 1,800+ jurisdictions.',
      icon: 'fact_check',
    },
  ];

  selectedRole: AuthRole = 'tenant';
  authMode: AuthMode = 'sign-in';
  showPassword = false;
  showConfirmPassword = false;
  email = '';
  password = '';
  isSubmitting = false;
  submitError = '';
  private readonly router = inject(Router);
  private readonly authSession = inject(AdminAuthSession);

  get selectedRoleMeta() {
    return this.roles.find((role) => role.key === this.selectedRole) ?? this.roles[0];
  }

  get ctaText(): string {
    const roleName = this.selectedRoleMeta.label;

    switch (this.authMode) {
      case 'register':
        return `Create ${roleName} account`;
      case 'reset':
        return 'Send reset link';
      default:
        return `Sign in to ${roleName} portal`;
    }
  }

  get authHeading(): string {
    switch (this.authMode) {
      case 'register':
        return 'Create your TrustRent account';
      case 'reset':
        return 'Reset your TrustRent access';
      default:
        return 'Sign in to TrustRent';
    }
  }

  get authDescription(): string {
    switch (this.authMode) {
      case 'register':
        return 'Set up a secure account and start managing verified rentals, leases, and compliance records.';
      case 'reset':
        return 'Choose the role tied to your existing TrustRent identity and receive a secure recovery link.';
      default:
        return 'Access your verified tenant dashboard, property portfolio, or deed audit workstation.';
    }
  }

  selectRole(role: AuthRole): void {
    this.selectedRole = role;
  }

  setMode(mode: AuthMode): void {
    this.authMode = mode;
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  toggleConfirmPasswordVisibility(): void {
    this.showConfirmPassword = !this.showConfirmPassword;
  }

  async onSubmit(): Promise<void> {
    if (this.authMode !== 'sign-in') {
      this.submitError = 'This authentication flow is not available yet.';
      return;
    }

    this.isSubmitting = true;
    this.submitError = '';
    try {
      await this.authSession.signIn(this.email, this.password);
      await this.router.navigateByUrl('/admin');
    } catch (error) {
      this.submitError = error instanceof Error && !(error instanceof HttpErrorResponse)
        ? error.message
        : error instanceof HttpErrorResponse && error.status === 401
          ? 'Email or password is incorrect.'
          : 'Unable to sign in. Please try again.';
    } finally {
      this.isSubmitting = false;
    }
  }
}
