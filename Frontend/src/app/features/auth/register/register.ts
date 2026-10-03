import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

import { AuthService } from '../../../core/auth/auth.service';
import { UserRole } from '../../../core/auth/auth.models';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink
  ],
  templateUrl: './register.html',
  styleUrl: './register.scss'
})
export class RegisterComponent {

  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  loading = false;
  errorMessage = '';
  successMessage = '';

  registerForm = this.fb.nonNullable.group({
    fullName: [
      '',
      [
        Validators.required,
        Validators.minLength(3)
      ]
    ],

    email: [
      '',
      [
        Validators.required,
        Validators.email
      ]
    ],

    phoneNumber: [
      '',
      [
        Validators.required,
        Validators.pattern(/^09\d{8}$/)
      ]
    ],

    password: [
      '',
      [
        Validators.required,
        Validators.minLength(8),
        Validators.pattern(
          /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$/
        )
      ]
    ],

    confirmPassword: [
      '',
      [
        Validators.required
      ]
    ],

    role: [
      'Tenant' as 'Tenant' | 'Landlord',
      Validators.required
    ]
  });

  get passwordsMatch(): boolean {
    return (
      this.registerForm.controls.password.value ===
      this.registerForm.controls.confirmPassword.value
    );
  }

  selectRole(role: 'Tenant' | 'Landlord'): void {
    this.registerForm.controls.role.setValue(role);
  }

  onSubmit(): void {

    this.errorMessage = '';
    this.successMessage = '';

    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    if (!this.passwordsMatch) {
      this.errorMessage = 'Passwords do not match.';
      return;
    }

    this.loading = true;

    const {
      fullName,
      email,
      phoneNumber,
      password,
      role
    } = this.registerForm.getRawValue();

    this.authService.register({
      fullName,
      email,
      phoneNumber,
      password,
      role
    }).subscribe({

      next: () => {

        this.loading = false;

        this.successMessage =
          'Your account has been created successfully. Please sign in.';

        setTimeout(() => {
          this.router.navigate(['/login']);
        }, 1200);
      },

      error: error => {

        this.loading = false;

        this.errorMessage =
          error?.error?.message ??
          'Registration failed. Please check your information and try again.';
      }
    });
  }
}