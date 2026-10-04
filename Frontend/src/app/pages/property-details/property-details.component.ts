import { DecimalPipe } from '@angular/common';
import { ChangeDetectorRef, Component, DestroyRef, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { catchError, EMPTY, finalize, switchMap, tap } from 'rxjs';
import { toApiErrorMessage } from '../../api-error';
import { Property, PropertyService } from '../../property.service';
import { PropertyStateComponent } from '../../shared/property-state.component';

@Component({
  selector: 'app-property-details',
  standalone: true,
  imports: [DecimalPipe, RouterLink, PropertyStateComponent],
  templateUrl: './property-details.component.html',
  styleUrl: './property-details.component.scss',
})
export class PropertyDetailsComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly propertyService = inject(PropertyService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly changeDetector = inject(ChangeDetectorRef);
  property: Property | null = null;
  loading = true;
  errorMessage = '';

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
}