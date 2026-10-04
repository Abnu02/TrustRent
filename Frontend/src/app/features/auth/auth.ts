import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AdminAuthSession } from '../Admin/services/admin-auth-session';
import { AuthIcon, AuthIconName } from './auth-icon';

type AuthMode = 'sign-in' | 'register' | 'reset';
type AuthRole = 'tenant' | 'landlord' | 'admin';

@Component({
  selector: 'app-auth',
  standalone: true,
  imports: [CommonModule, FormsModule, AuthIcon],
  templateUrl: './auth.html',
})
export class Auth {
  readonly authModes: ReadonlyArray<{ key: AuthMode; label: string; description: string; icon: AuthIconName }> = [
    { key: 'sign-in', label: 'Sign in', description: 'Access your TrustRent account.', icon: 'login' },
    { key: 'register', label: 'Create account', description: 'Set up a new TrustRent account.', icon: 'person_add' },
    { key: 'reset', label: 'Reset password', description: 'Recover access to your account.', icon: 'lock_reset' },
  ];

  readonly roles: ReadonlyArray<{
    key: AuthRole;
    label: string;
    subtitle: string;
    icon: AuthIconName;
  }> = [
    { key: 'tenant', label: 'Tenant', subtitle: 'Find a place to rent', icon: 'person' },
    { key: 'landlord', label: 'Landlord', subtitle: 'Manage your listings', icon: 'home_work' },
    { key: 'admin', label: 'Admin', subtitle: 'Review submissions', icon: 'admin_panel_settings' },
  ];

  readonly trustHighlights: ReadonlyArray<{ title: string; description: string; icon: AuthIconName }> = [
    {
      title: 'Property review',
      description: 'Property submissions are reviewed before they are approved for listing.',
      icon: 'fact_check',
    },
    {
      title: 'Clear decisions',
      description: 'Review outcomes and requests are recorded so important changes are easy to follow.',
      icon: 'rule',
    },
    {
      title: 'One connected workspace',
      description: 'Keep account access and property review information together in one place.',
      icon: 'dashboard',
    },
  ];

  selectedRole: AuthRole = 'admin';
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
        return 'Create a TrustRent account to get started with property information and reviews.';
      case 'reset':
        return 'Enter your account email to get help restoring access to TrustRent.';
      default:
        return 'Sign in to your TrustRent account and continue to the workspace for your role.';
    }
  }

  selectRole(role: AuthRole): void {
    this.selectedRole = role;
  }

  setMode(mode: AuthMode): void {
    this.authMode = mode;
    if (mode === 'sign-in') {
      this.selectedRole = 'admin';
    } else if (this.selectedRole === 'admin') {
      this.selectedRole = 'tenant';
    }
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
      if (error instanceof HttpErrorResponse && error.status === 401) {
        this.submitError = 'Email or password is incorrect.';
      } else if (error instanceof HttpErrorResponse && error.status >= 500) {
        this.submitError = 'The sign-in service is unavailable. Please check the backend database connection and try again.';
      } else if (error instanceof Error && !(error instanceof HttpErrorResponse)) {
        this.submitError = error.message;
      } else {
        this.submitError = 'Unable to sign in. Please try again.';
      }
    } finally {
      this.isSubmitting = false;
    }
  }
}
