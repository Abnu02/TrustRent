import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  signal
} from '@angular/core';

import { DecimalPipe } from '@angular/common';
import { Router } from '@angular/router';

import { PropertiesStore } from '../../store/properties.store';
import { PropertyCardComponent } from '../../components/property-card/property-card.component';
import { TenantNavbarComponent } from '../../components/tenant-navbar/tenant-navbar.component';

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
export class TenantDashboardComponent {

  private readonly store = inject(PropertiesStore);
  private readonly router = inject(Router);

  readonly properties = this.store.properties;

  readonly location = signal('');
  readonly selectedBedrooms = signal<number | null>(null);

  readonly minRent = signal(0);
  readonly maxRent = signal(50000);

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
    this.maxRent.set(50000);
  }

  viewProperty(id: string): void {
    this.router.navigate([
      '/tenant/properties',
      id
    ]);
  }
}