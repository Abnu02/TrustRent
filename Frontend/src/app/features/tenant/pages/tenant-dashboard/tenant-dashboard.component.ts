import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  OnInit,
  signal
} from '@angular/core';

import { HttpErrorResponse } from '@angular/common/http';
import { DecimalPipe } from '@angular/common';
import { Router } from '@angular/router';

import { PropertyCardComponent } from '../../components/property-card/property-card.component';
import { TenantNavbarComponent } from '../../components/tenant-navbar/tenant-navbar.component';
import { TenantPropertyService } from '../../services/tenant-property.service';
import { Property } from '../../../../core/models/property.model';
import { AuthService } from '../../../../core/auth/auth.service';

@Component({
  selector: 'app-tenant-dashboard',
  standalone: true,
  imports: [
    DecimalPipe,
    TenantNavbarComponent,
    PropertyCardComponent
  ],
  templateUrl: './tenant-dashboard.component.html',
  styleUrl: './tenant-dashboard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TenantDashboardComponent implements OnInit {

  private readonly propertyService = inject(TenantPropertyService);
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);

  readonly properties = signal<Property[]>([]);
  readonly loading = signal(true);
  readonly errorMessage = signal('');
  readonly userName = this.authService.currentUser()?.fullName ?? 'Tenant';

  readonly location = signal('');
  readonly selectedBedrooms = signal<number | null>(null);

  readonly minRent = signal(0);
  readonly maxRent = signal(50000);
  private readonly defaultMaxRent = signal(50000);

  readonly bedroomOptions = [1, 2, 3, 4];

  readonly filteredProperties = computed(() => {

    const locationValue =
      this.location().trim().toLowerCase();

    const bedrooms =
      this.selectedBedrooms();

    const min =
      this.minRent();

    const max =
      this.maxRent();

    return this.properties().filter(property => {

      const matchesLocation =
        !locationValue ||
        property.location
          .toLowerCase()
          .includes(locationValue);

      const matchesBedrooms =
        bedrooms === null ||
        property.bedrooms === bedrooms;

      const matchesPrice =
        property.rent >= min &&
        property.rent <= max;

      return (
        matchesLocation &&
        matchesBedrooms &&
        matchesPrice
      );
    });
  });

  ngOnInit(): void {
    this.loadProperties();
  }

  loadProperties(): void {
    this.loading.set(true);
    this.errorMessage.set('');

    this.propertyService.getVerifiedProperties().subscribe({
      next: properties => {
        this.properties.set(properties.filter(property => property.isVerified));
        const maxRent = Math.max(
          50000,
          ...properties.map(property => property.rent)
        );
        this.defaultMaxRent.set(maxRent);
        this.maxRent.set(maxRent);
        this.loading.set(false);
      },
      error: error => {
        this.errorMessage.set(this.getErrorMessage(error));
        this.loading.set(false);
      }
    });
  }

  setLocation(value: string): void {
    this.location.set(value);
  }

  setBedrooms(value: number | null): void {
    this.selectedBedrooms.set(value);
  }

  setMinRent(value: number): void {
    this.minRent.set(value);
  }

  setMaxRent(value: number): void {
    this.maxRent.set(value);
  }

  resetFilters(): void {
    this.location.set('');
    this.selectedBedrooms.set(null);
    this.minRent.set(0);
    this.maxRent.set(this.defaultMaxRent());
  }

  viewProperty(id: string): void {
    this.router.navigate([
      '/tenant/properties',
      id
    ]);
  }

  private getErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      if (error.status === 0) {
        return 'Unable to connect to the property service. Check that the backend is running.';
      }
      if (error.status === 401 || error.status === 403) {
        return 'Your session is no longer valid. Please sign in again.';
      }
      return error.error?.message ??
        `Unable to load verified properties (HTTP ${error.status}).`;
    }

    return 'Unable to load verified properties.';
  }
}