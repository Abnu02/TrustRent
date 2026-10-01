import { ChangeDetectorRef, Component, DestroyRef, inject } from '@angular/core';
import { Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { PropertyService, PropertyDraft } from '../../property.service';
import { FeedbackService } from '../../feedback.service';
import { ApiErrorMessage, toApiErrorMessage } from '../../api-error';
import { PropertyEditorComponent } from '../../shared/property-editor.component';

@Component({
  selector: 'app-create-property',
  standalone: true,
  imports: [PropertyEditorComponent],
  templateUrl: './create-property.component.html',
})
export class CreatePropertyComponent {
  private readonly propertyService = inject(PropertyService);
  private readonly feedback = inject(FeedbackService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly changeDetector = inject(ChangeDetectorRef);
  saving = false;
  errorMessage = '';
  validationMessages: string[] = [];

  createProperty(draft: PropertyDraft): void {
    this.saving = true;
    this.errorMessage = '';
    this.validationMessages = [];
    this.propertyService.create(draft).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: property => {
        this.saving = false;
        this.feedback.success('Property created successfully.');
        this.changeDetector.markForCheck();
        void this.router.navigate(['/landlord/properties', property.id]);
      },
      error: (error: unknown) => this.handleError(error),
    });
  }

  cancel(): void {
    void this.router.navigate(['/landlord/properties']);
  }

  private handleError(error: unknown): void {
    const apiError: ApiErrorMessage = toApiErrorMessage(error);
    this.errorMessage = apiError.message;
    this.validationMessages = apiError.validationMessages;
    this.saving = false;
    this.changeDetector.markForCheck();
  }
}