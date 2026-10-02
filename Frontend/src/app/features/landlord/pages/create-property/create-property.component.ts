import { Component, inject } from '@angular/core';
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
    'Condominium',
    'Villa'
  ];


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
   * This is currently frontend-only.
   * Backend image upload will be implemented later.
   */
  onImageSelected(event: Event): void {

    const input = event.target as HTMLInputElement;

    if (!input.files || input.files.length === 0) {
      return;
    }

    const file = input.files[0];

    /*
     * Only allow common image formats.
     */
    if (!file.type.startsWith('image/')) {
      this.errorMessage = 'Please select a valid image file.';
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


    /*
     * IMPORTANT:
     *
     * We only send the fields documented by
     * POST /api/v1/properties.
     *
     * The image is NOT sent yet because
     * the backend image endpoint has not
     * been implemented.
     *
     * landlordId is also NOT sent.
     */
    const request = this.propertyForm.getRawValue();


    this.propertyService
      .createProperty(request)
      .subscribe({

        next: (response) => {

          this.isSubmitting = false;

          this.successMessage =
            'Property created successfully and is waiting for verification.';

          /*
           * For now the image preview is local only.
           * Backend image support will be connected later.
           */

          console.log('Created property:', response);

          /*
           * Go to My Properties after a short delay.
           */
          setTimeout(() => {

            this.router.navigate([
              '/landlord/my-properties'
            ]);

          }, 1200);
        },

        error: (error) => {

          this.isSubmitting = false;

          console.error(
            'Create property error:',
            error
          );

          this.errorMessage =
            error?.error?.message ??
            'Unable to create the property. Please try again.';
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
}