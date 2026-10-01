import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, inject } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Property, PropertyDraft, PropertyType } from '../property.service';

const MAX_PROPERTY_VALUE = 9_999_999_999.99;

@Component({
  selector: 'app-property-editor',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './property-editor.component.html',
  styleUrl: './property-editor.component.scss',
})
export class PropertyEditorComponent implements OnChanges {
  @Input() mode: 'create' | 'edit' = 'create';
  @Input() property: Property | null = null;
  @Input() saving = false;
  @Input() errorMessage = '';
  @Input() validationMessages: string[] = [];
  @Output() submitted = new EventEmitter<PropertyDraft>();
  @Output() cancelled = new EventEmitter<void>();

  readonly form = new FormGroup({
    title: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(200)] }),
    description: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(2000)] }),
    propertyType: new FormControl<PropertyType>('Apartment', { nonNullable: true, validators: [Validators.required] }),
    rent: new FormControl(0, { nonNullable: true, validators: [Validators.required, Validators.min(0.01), Validators.max(MAX_PROPERTY_VALUE)] }),
    deposit: new FormControl(0, { nonNullable: true, validators: [Validators.required, Validators.min(0), Validators.max(MAX_PROPERTY_VALUE)] }),
    location: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(200)] }),
    bedrooms: new FormControl(0, { nonNullable: true, validators: [Validators.required, Validators.min(0), Validators.max(100), Validators.pattern(/^\d+$/)] }),
    bathrooms: new FormControl(0, { nonNullable: true, validators: [Validators.required, Validators.min(0), Validators.max(100), Validators.pattern(/^\d+$/)] }),
  });

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['property'] && this.property) {
      this.form.reset({
        title: this.property.title,
        description: this.property.description,
        propertyType: this.property.propertyType,
        rent: this.property.rent,
        deposit: this.property.deposit,
        location: this.property.location,
        bedrooms: this.property.bedrooms,
        bathrooms: this.property.bathrooms,
      });
    }
  }

  submit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid || this.saving) return;
    this.submitted.emit(this.form.getRawValue());
  }
}