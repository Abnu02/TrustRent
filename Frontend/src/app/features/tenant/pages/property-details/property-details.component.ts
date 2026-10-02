import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  inject,
  signal
} from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DecimalPipe } from '@angular/common';

import { PropertyDetails } from '../../../../core/models/property.model';
import { PropertiesStore } from '../../store/properties.store';
import { TenantNavbarComponent } from '../../components/tenant-navbar/tenant-navbar.component';

@Component({
  selector: 'app-property-details',
  standalone: true,
  imports: [
    DecimalPipe,
    TenantNavbarComponent
  ],
  templateUrl: './property-details.component.html',
  styleUrl: './property-details.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PropertyDetailsComponent implements OnInit {

  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly store = inject(PropertiesStore);

  readonly property = signal<PropertyDetails | null>(null);

  ngOnInit(): void {

    const id =
      this.route.snapshot.paramMap.get('id');

    if (!id) {
      this.router.navigate(['/tenant']);
      return;
    }

    const property =
      this.store.getPropertyById(id);

    if (!property) {
      this.router.navigate(['/tenant']);
      return;
    }

    this.property.set({
      ...property,

      description:
        `Clean and spacious ${property.propertyType.toLowerCase()} located in ${property.location}. Perfect for comfortable modern living with convenient access to nearby services.`,

      status: 'Approved',

      verifiedAt:
        '2026-10-01T09:15:00Z',

      landlord: {
        id: 'landlord-001',
        fullName: 'Abreham Bekele',
        phoneNumber: '0912345678',
        email: 'abreham@example.com',
        isVerified: true
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/tenant']);
  }
}