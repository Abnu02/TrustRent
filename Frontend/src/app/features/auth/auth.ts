import { ChangeDetectorRef, Component, DestroyRef, inject } from '@angular/core';
import { ReactiveFormsModule, FormControl, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AuthService } from '../../auth/auth.service';
import { toApiErrorMessage } from '../../api-error';

type AuthMode = 'sign-in' | 'register';

@Component({
  selector: 'app-auth',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  styleUrl: './auth.scss',
  templateUrl: './auth.html',
})
export class Auth {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly changeDetector = inject(ChangeDetectorRef);

  mode: AuthMode = 'sign-in';
  submitting = false;
  errorMessage = '';
  successMessage = '';

  readonly loginForm = new FormGroup({
    email: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.email] }),
    password: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
  });

  readonly registerForm = new FormGroup({
    fullName: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(200)] }),
    email: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.email, Validators.maxLength(256)] }),
    phoneNumber: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.pattern(/^[+()\d -]{7,50}$/)] }),
    password: new FormControl('', { nonNullable: true, validators: [
      Validators.required,
      Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$/),
    ] }),
    confirmPassword: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
  }, { validators: passwordsMatch });

  setMode(mode: AuthMode): void {
    this.mode = mode;
    this.errorMessage = '';
    this.successMessage = '';
  }

  submit(): void {
    const form = this.mode === 'sign-in' ? this.loginForm : this.registerForm;
    form.markAllAsTouched();
    if (form.invalid || this.submitting) return;

    this.submitting = true;
    this.errorMessage = '';
    if (this.mode === 'sign-in') {
      const { email, password } = this.loginForm.getRawValue();
      this.auth.login({ email, password }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: response => {
          this.submitting = false;
          this.changeDetector.markForCheck();
          void this.router.navigate(response.user.role.toLowerCase() === 'landlord' ? ['/landlord/dashboard'] : ['/auth']);
        },
        error: error => this.handleError(error),
      });
      return;
    }

    const { fullName, email, phoneNumber, password } = this.registerForm.getRawValue();
    this.auth.register({ fullName, email, phoneNumber, password, role: 'Landlord' })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.submitting = false;
          this.successMessage = 'Your landlord account is ready. Sign in to continue.';
          this.loginForm.controls.email.setValue(email);
          this.mode = 'sign-in';
          this.registerForm.reset();
          this.changeDetector.markForCheck();
        },
        error: error => this.handleError(error),
      });
  }

  private handleError(error: unknown): void {
    this.submitting = false;
    const apiError = toApiErrorMessage(error);
    this.errorMessage = apiError.validationMessages.length
      ? `${apiError.message} ${apiError.validationMessages.join(' ')}`
      : apiError.message;
    this.changeDetector.markForCheck();
  }
}

function passwordsMatch(control: AbstractControl): ValidationErrors | null {
  const password = control.get('password')?.value;
  const confirmation = control.get('confirmPassword')?.value;
  return !password || !confirmation || password === confirmation ? null : { passwordMismatch: true };
}