import { Component, inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

import { LandlordPropertyService } from '../../services/landlord-property.service';

@Component({
  selector: 'app-create-property',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink
  ],
  templateUrl: './create-property.component.html',
  styleUrl: './create-property.component.scss'
})
export class CreatePropertyComponent {

  private readonly fb = inject(FormBuilder);
  private readonly propertyService = inject(LandlordPropertyService);
  private readonly router = inject(Router);

  isSubmitting = false;

  errorMessage = '';

  successMessage = '';

  selectedImage: File | null = null;

  imagePreview: string | null = null;


  /**
   * Property types used by the frontend.
   *
   * The backend documentation uses "Apartment"
   * in its example.
   */
  readonly propertyTypes = [
    'Apartment',
    'House',
    'Studio',
    'Villa',
    'Other'
  ] as const;


  /**
   * Create Property Form
   */
  propertyForm = this.fb.nonNullable.group({

    title: [
      '',
      [
        Validators.required,
        Validators.minLength(3),
        Validators.maxLength(150)
      ]
    ],

    description: [
      '',
      [
        Validators.required,
        Validators.minLength(10),
        Validators.maxLength(1000)
      ]
    ],

    propertyType: [
      'Apartment',
      [
        Validators.required
      ]
    ],

    rent: [
      0,
      [
        Validators.required,
        Validators.min(1)
      ]
    ],

    deposit: [
      0,
      [
        Validators.required,
        Validators.min(0)
      ]
    ],

    location: [
      '',
      [
        Validators.required,
        Validators.minLength(2),
        Validators.maxLength(200)
      ]
    ],

    bedrooms: [
      1,
      [
        Validators.required,
        Validators.min(0),
        Validators.max(50)
      ]
    ],

    bathrooms: [
      1,
      [
        Validators.required,
        Validators.min(0),
        Validators.max(50)
      ]
    ]

  });


  /**
   * Convenient form controls.
   */
  get title() {
    return this.propertyForm.controls.title;
  }

  get description() {
    return this.propertyForm.controls.description;
  }

  get propertyType() {
    return this.propertyForm.controls.propertyType;
  }

  get rent() {
    return this.propertyForm.controls.rent;
  }

  get deposit() {
    return this.propertyForm.controls.deposit;
  }

  get location() {
    return this.propertyForm.controls.location;
  }

  get bedrooms() {
    return this.propertyForm.controls.bedrooms;
  }

  get bathrooms() {
    return this.propertyForm.controls.bathrooms;
  }


  /**
   * Handle property image selection.
   *
   * Keep the selected image for upload after the property is created.
   */
  onImageSelected(event: Event): void {

    const input = event.target as HTMLInputElement;

    if (!input.files || input.files.length === 0) {
      return;
    }

    const file = input.files[0];
    const allowedTypes = ['image/jpeg', 'image/png'];

    if (!allowedTypes.includes(file.type)) {
      this.errorMessage = 'Please select a JPG or PNG image.';
      return;
    }

    /*
     * Limit frontend image selection to 5 MB.
     */
    const maxSize = 5 * 1024 * 1024;

    if (file.size > maxSize) {
      this.errorMessage = 'Image size must be less than 5 MB.';
      return;
    }

    this.errorMessage = '';

    this.selectedImage = file;

    /*
     * Create a temporary browser preview.
     */
    const reader = new FileReader();

    reader.onload = () => {
      this.imagePreview = reader.result as string;
    };

    reader.onerror = () => {
      this.errorMessage = 'Unable to preview the selected image.';
      this.removeImage();
    };

    reader.readAsDataURL(file);
  }


  /**
   * Remove selected image.
   */
  removeImage(): void {

    this.selectedImage = null;

    this.imagePreview = null;
  }


  /**
   * Submit the property.
   */
  onSubmit(): void {

    this.errorMessage = '';
    this.successMessage = '';

    /*
     * Stop submission when the form is invalid.
     */
    if (this.propertyForm.invalid) {

      this.propertyForm.markAllAsTouched();

      return;
    }

    this.isSubmitting = true;


    const request = {
      ...this.propertyForm.getRawValue(),
      propertyType: this.propertyForm.controls.propertyType.value
    };


    this.propertyService
      .createProperty(request)
      .subscribe({

        next: response => {
          if (!this.selectedImage) {
            this.finishCreation();
            return;
          }

          this.propertyService
            .uploadImage(response.id, this.selectedImage)
            .subscribe({
              next: () => this.finishCreation(),
              error: error => {
                this.isSubmitting = false;
                this.errorMessage =
                  this.getErrorMessage(error) +
                  ' The property was created; you can add its image from My Properties later.';
              }
            });
        },

        error: (error) => {
          this.isSubmitting = false;
          this.errorMessage = this.getErrorMessage(error);
        }

      });
  }


  /**
   * Cancel and return to dashboard.
   */
  cancel(): void {

    this.router.navigate([
      '/landlord/dashboard'
    ]);
  }

  private finishCreation(): void {
    this.isSubmitting = false;
    this.successMessage =
      'Property created successfully and is waiting for verification.';

    setTimeout(() => {
      this.router.navigate(['/landlord/my-properties']);
    }, 900);
  }

  private getErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      if (error.status === 0) {
        return 'Cannot connect to the API. Check that the backend is running.';
      }

      if (error.status === 401 || error.status === 403) {
        return error.error?.message ??
          'Your landlord account must be verified before you can create properties.';
      }

      const validationErrors = error.error?.errors;
      if (validationErrors && typeof validationErrors === 'object') {
        const messages = Object.values(validationErrors)
          .flatMap(value => Array.isArray(value) ? value : [])
          .filter((value): value is string => typeof value === 'string');

        if (messages.length > 0) {
          return messages.join(' ');
        }
      }

      return error.error?.message ??
        `Unable to create the property (HTTP ${error.status}).`;
    }

    return 'Unable to create the property. Please try again.';
  }
}