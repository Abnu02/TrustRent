import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { vi } from 'vitest';
import { AuthService } from '../../auth/auth.service';
import { Auth } from './auth';

describe('Auth', () => {
  let component: Auth;
  let fixture: ComponentFixture<Auth>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Auth],
    }).compileComponents();

    fixture = TestBed.createComponent(Auth);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('registers a tenant account and reports success', async () => {
    const authService = TestBed.inject(AuthService);
    const register = vi.spyOn(authService, 'register').mockReturnValue(of({ message: 'Registration successful' }));

    component.authMode = 'register';
    component.fullName = 'Taylor Tenant';
    component.email = 'taylor@example.com';
    component.phoneNumber = '555-0100';
    component.password = 'ValidPass1!';
    component.confirmPassword = 'ValidPass1!';

    await component.onSubmit();

    expect(register).toHaveBeenCalledWith({
      fullName: 'Taylor Tenant',
      email: 'taylor@example.com',
      phoneNumber: '555-0100',
      password: 'ValidPass1!',
      role: 'Tenant',
    });
    expect(component.submitSuccess).toBe('Your account was created. You can now sign in.');
    expect(component.password).toBe('');
    expect(component.confirmPassword).toBe('');
  });

  it('does not submit registration when passwords do not match', async () => {
    const authService = TestBed.inject(AuthService);
    const register = vi.spyOn(authService, 'register');
    component.authMode = 'register';
    component.password = 'ValidPass1!';
    component.confirmPassword = 'different';

    await component.onSubmit();

    expect(register).not.toHaveBeenCalled();
    expect(component.submitError).toBe('Passwords do not match.');
  });

  it('signs in landlords to the landlord workspace', async () => {
    const authService = TestBed.inject(AuthService);
    const login = vi.spyOn(authService, 'login').mockReturnValue(of({
      accessToken: 'token',
      user: {
        id: 'landlord-id',
        fullName: 'Taylor Landlord',
        email: 'taylor@example.com',
        role: 'Landlord',
        isVerified: false,
      },
    }));
    const navigateByUrl = vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);
    component.selectedRole = 'landlord';
    component.email = 'taylor@example.com';
    component.password = 'ValidPass1!';

    await component.onSubmit();

    expect(login).toHaveBeenCalledWith({ email: 'taylor@example.com', password: 'ValidPass1!' });
    expect(navigateByUrl).toHaveBeenCalledWith('/landlord');
  });

  it('signs in tenants to the coming-soon workspace', async () => {
    const authService = TestBed.inject(AuthService);
    vi.spyOn(authService, 'login').mockReturnValue(of({
      accessToken: 'token',
      user: {
        id: 'tenant-id',
        fullName: 'Taylor Tenant',
        email: 'taylor@example.com',
        role: 'Tenant',
        isVerified: false,
      },
    }));
    const navigateByUrl = vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);
    component.selectedRole = 'tenant';

    await component.onSubmit();

    expect(navigateByUrl).toHaveBeenCalledWith('/tenant');
  });

  it('rejects sign-in when the selected role does not match the account role', async () => {
    const authService = TestBed.inject(AuthService);
    vi.spyOn(authService, 'login').mockReturnValue(of({
      accessToken: 'token',
      user: {
        id: 'landlord-id',
        fullName: 'Taylor Landlord',
        email: 'taylor@example.com',
        role: 'Landlord',
        isVerified: false,
      },
    }));
    const clearSession = vi.spyOn(authService, 'clearSession');
    const navigateByUrl = vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);
    component.selectedRole = 'admin';

    await component.onSubmit();

    expect(clearSession).toHaveBeenCalled();
    expect(navigateByUrl).not.toHaveBeenCalled();
    expect(component.submitError).toBe('This account is registered as Landlord. Select that role and try again.');
  });
});
