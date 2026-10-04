import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  inject,
  signal
} from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { DecimalPipe } from '@angular/common';

import { PropertyDetails } from '../../../../core/models/property.model';
import { TenantNavbarComponent } from '../../components/tenant-navbar/tenant-navbar.component';
import { TenantPropertyService } from '../../services/tenant-property.service';

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
  private readonly propertyService = inject(TenantPropertyService);

  readonly property = signal<PropertyDetails | null>(null);
  readonly loading = signal(true);
  readonly errorMessage = signal('');

  ngOnInit(): void {

    const id =
      this.route.snapshot.paramMap.get('id');

    if (!id) {
      this.router.navigate(['/tenant']);
      return;
    }

    this.loading.set(true);
    this.propertyService.getVerifiedProperty(id).subscribe({
      next: property => {
        this.property.set(property);
        this.loading.set(false);
      },
      error: error => {
        this.errorMessage.set(this.getErrorMessage(error));
        this.loading.set(false);
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/tenant']);
  }

  private getErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      if (error.status === 0) {
        return 'Unable to connect to the property service. Check that the backend is running.';
      }
      if (error.status === 404) {
        return 'This property is no longer available or has not been verified.';
      }
      if (error.status === 401 || error.status === 403) {
        return 'Your session is no longer valid. Please sign in again.';
      }
      return error.error?.message ??
        `Unable to load property details (HTTP ${error.status}).`;
    }

    return 'Unable to load property details.';
  }
}