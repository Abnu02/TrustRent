import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { AuthService } from './auth.service';
import { authInterceptor } from './auth.interceptor';

describe('AuthService and authInterceptor', () => {
  let service: AuthService;
  let http: HttpTestingController;
  const token = createToken(Date.now() + 60_000);

  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        provideRouter([]),
      ],
    });
    service = TestBed.inject(AuthService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
    sessionStorage.clear();
  });

  it('stores the returned access token and landlord profile after login', () => {
    service.login({ email: 'landlord@example.test', password: 'TestPassword!9' }).subscribe();
    const request = http.expectOne('/api/v1/auth/login');
    expect(request.request.headers.has('Authorization')).toBe(false);
    request.flush({ accessToken: token, user: landlordUser });

    expect(service.isAuthenticated).toBe(true);
    expect(service.hasRole('Landlord')).toBe(true);
    expect(sessionStorage.getItem('trustrent.accessToken')).toBe(token);
  });

  it('registers only a landlord with the API request shape', () => {
    service.register({
      fullName: 'Landlord Example',
      email: 'landlord@example.test',
      phoneNumber: '+251900000000',
      password: 'TestPassword!9',
      role: 'Landlord',
    }).subscribe();

    const request = http.expectOne('/api/v1/auth/register');
    expect(request.request.method).toBe('POST');
    expect(request.request.body.role).toBe('Landlord');
    request.flush({ message: 'Landlord account created.' });
  });

  it('adds the bearer token to property API requests', () => {
    service.login({ email: landlordUser.email, password: 'TestPassword!9' }).subscribe();
    http.expectOne('/api/v1/auth/login').flush({ accessToken: token, user: landlordUser });

    TestBed.inject(HttpClient).get('/api/v1/properties/my').subscribe();
    const request = http.expectOne('/api/v1/properties/my');
    expect(request.request.headers.get('Authorization')).toBe(`Bearer ${token}`);
    request.flush({ items: [], page: 1, pageSize: 10, totalCount: 0, totalPages: 0 });
  });
});

const landlordUser = {
  id: 'user-id',
  fullName: 'Landlord Example',
  email: 'landlord@example.test',
  role: 'Landlord',
  isVerified: false,
};

function createToken(expiration: number): string {
  return `header.${btoa(JSON.stringify({ exp: Math.floor(expiration / 1000) }))}.signature`;
}