import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { App } from './app';
import { routes } from './app.routes';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideRouter(routes)],
    })
      .compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should render the router outlet root', async () => {
    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('router-outlet')).not.toBeNull();
  });

  it('should declare all landlord property routes', () => {
    expect(routes.some(route => route.path === 'auth')).toBe(true);
    const landlordRoute = routes.find(route => route.path === 'landlord');
    expect(landlordRoute?.canActivate).toBeTruthy();
    expect(landlordRoute?.children?.map(route => route.path)).toEqual([
      '', 'dashboard', 'properties/new', 'properties/:id/edit', 'properties/:id', 'properties',
    ]);
  });

  it('should protect the tenant coming-soon route', () => {
    const tenantRoute = routes.find(route => route.path === 'tenant');
    expect(tenantRoute?.canActivate).toBeTruthy();
    expect(tenantRoute?.component).toBeTruthy();
  });
});
