import { ChangeDetectorRef, Component, DestroyRef, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { catchError, EMPTY, finalize, switchMap, tap } from 'rxjs';
import { ApiErrorMessage, toApiErrorMessage } from '../../api-error';
import { FeedbackService } from '../../feedback.service';
import { Property, PropertyDraft, PropertyService } from '../../property.service';
import { PropertyStateComponent } from '../../shared/property-state.component';
import { PropertyEditorComponent } from '../../shared/property-editor.component';

@Component({
  selector: 'app-edit-property',
  standalone: true,
  imports: [RouterLink, PropertyStateComponent, PropertyEditorComponent],
  templateUrl: './edit-property.component.html',
})
export class EditPropertyComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly propertyService = inject(PropertyService);
  private readonly feedback = inject(FeedbackService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly changeDetector = inject(ChangeDetectorRef);
  property: Property | null = null;
  loading = true;
  saving = false;
  errorMessage = '';
  validationMessages: string[] = [];

  ngOnInit(): void {
    this.route.paramMap.pipe(
      switchMap(params => {
        const id = params.get('id');
        if (!id) return EMPTY;
        this.loading = true;
        this.errorMessage = '';
        return this.propertyService.getById(id).pipe(
          tap(property => {
            this.property = property;
            this.changeDetector.markForCheck();
          }),
          catchError(error => {
            this.errorMessage = toApiErrorMessage(error).message;
            this.changeDetector.markForCheck();
            return EMPTY;
          }),
          finalize(() => {
            this.loading = false;
            this.changeDetector.markForCheck();
          }),
        );
      }),
      takeUntilDestroyed(this.destroyRef),
    ).subscribe();
  }

  saveProperty(draft: PropertyDraft): void {
    if (!this.property) return;
    this.saving = true;
    this.errorMessage = '';
    this.validationMessages = [];
    this.propertyService.update(this.property.id, draft).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: property => {
        this.saving = false;
        this.feedback.success('Property changes saved.');
        this.changeDetector.markForCheck();
        void this.router.navigate(['/landlord/properties', property.id]);
      },
      error: (error: unknown) => {
        const apiError: ApiErrorMessage = toApiErrorMessage(error);
        this.errorMessage = apiError.message;
        this.validationMessages = apiError.validationMessages;
        this.saving = false;
        this.changeDetector.markForCheck();
      },
    });
  }

  cancel(): void {
    if (this.property) void this.router.navigate(['/landlord/properties', this.property.id]);
  }
}