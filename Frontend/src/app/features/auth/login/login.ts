import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink
  ],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class LoginComponent {

  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  loading = false;
  errorMessage = '';

  loginForm = this.fb.nonNullable.group({
    email: [
      '',
      [
        Validators.required,
        Validators.email
      ]
    ],

    password: [
      '',
      [
        Validators.required
      ]
    ]
  });

  onSubmit(): void {

    this.errorMessage = '';

    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.loading = true;

    this.authService.login(this.loginForm.getRawValue())
      .subscribe({
        next: response => {

          this.authService.saveSession(response);

          this.navigateByRole(response.role);
        },

        error: error => {

          this.loading = false;

          this.errorMessage =
            error?.error?.message ??
            'Invalid email or password. Please try again.';
        }
      });
  }

  private navigateByRole(role: string): void {

    switch (role) {

      case 'Admin':
        this.router.navigate(['/admin']);
        break;

      case 'Landlord':
        this.router.navigate(['/landlord/dashboard']);
        break;

      case 'Tenant':
        this.router.navigate(['/tenant']);
        break;

      default:
        this.router.navigate(['/']);
    }
  }
}