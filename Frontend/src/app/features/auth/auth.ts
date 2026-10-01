import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

type AuthMode = 'sign-in' | 'register' | 'reset';
type AuthRole = 'tenant' | 'landlord' | 'auditor';

@Component({
  selector: 'app-auth',
  standalone: true,
  imports: [CommonModule],
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
    { key: 'auditor', label: 'Auditor', subtitle: 'County title records', icon: 'policy' },
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

  onSubmit(): void {
    // Intentionally left as a UI placeholder until the authentication API is wired in.
  }
}
