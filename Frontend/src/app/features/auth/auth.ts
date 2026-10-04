import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { AuthService, RegisterRequest } from '../../auth/auth.service';
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
  fullName = '';
  email = '';
  phoneNumber = '';
  password = '';
  confirmPassword = '';
  isSubmitting = false;
  submitError = '';
  submitSuccess = '';
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);

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
    this.submitError = '';
    this.submitSuccess = '';
    if (mode !== 'sign-in' && this.selectedRole === 'admin') {
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
    if (this.authMode === 'reset') {
      this.submitError = 'This authentication flow is not available yet.';
      return;
    }

    this.isSubmitting = true;
    this.submitError = '';
    this.submitSuccess = '';
    try {
      if (this.authMode === 'register') {
        if (this.password !== this.confirmPassword) {
          this.submitError = 'Passwords do not match.';
          return;
        }

        const request: RegisterRequest = {
          fullName: this.fullName.trim(),
          email: this.email.trim(),
          phoneNumber: this.phoneNumber.trim(),
          password: this.password,
          role: this.selectedRole === 'landlord' ? 'Landlord' : 'Tenant',
        };
        await firstValueFrom(this.authService.register(request));
        this.submitSuccess = 'Your account was created. You can now sign in.';
        this.password = '';
        this.confirmPassword = '';
        return;
      }

      const response = await firstValueFrom(
        this.authService.login({ email: this.email.trim(), password: this.password }),
      );
      const role = response.user.role.toLowerCase();
      if (role !== this.selectedRole) {
        this.authService.clearSession();
        this.submitError = `This account is registered as ${response.user.role}. Select that role and try again.`;
        return;
      }

      const destination = role === 'admin' ? '/admin' : role === 'landlord' ? '/landlord' : '/tenant';
      await this.router.navigateByUrl(destination);
    } catch (error) {
      if (this.authMode === 'register' && error instanceof HttpErrorResponse) {
        this.submitError = getRegistrationErrorMessage(error);
      } else if (error instanceof HttpErrorResponse && error.status === 401) {
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

function getRegistrationErrorMessage(error: HttpErrorResponse): string {
  const response = error.error;
  if (typeof response === 'object' && response !== null) {
    const body = response as Record<string, unknown>;
    const message = body['error'] ?? body['Error'];
    if (typeof message === 'string' && message.length > 0) {
      return message;
    }
  }

  if (error.status === 0) {
    return 'Unable to reach the registration service. Check the backend connection and try again.';
  }
  if (error.status >= 500) {
    return 'The registration service encountered an error. Please try again later.';
  }
  return 'Unable to create the account. Check the submitted details and try again.';
}
