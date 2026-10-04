import { ChangeDetectorRef, Component, DestroyRef, OnInit, inject } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { catchError, of } from 'rxjs';
import { toApiErrorMessage } from '../../api-error';
import { PropertyService, DashboardSnapshot } from '../../property.service';
import { PropertyStateComponent } from '../../shared/property-state.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [DecimalPipe, RouterLink, PropertyStateComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit {
  private readonly propertyService = inject(PropertyService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly changeDetector = inject(ChangeDetectorRef);
  snapshot: DashboardSnapshot | null = null;
  loading = true;
  errorMessage = '';

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.loading = true;
    this.errorMessage = '';
    this.propertyService.getDashboardSnapshot().pipe(
      catchError(error => {
        this.errorMessage = toApiErrorMessage(error).message;
        this.changeDetector.markForCheck();
        return of(null);
      }),
      takeUntilDestroyed(this.destroyRef),
    ).subscribe(snapshot => {
      this.snapshot = snapshot;
      this.loading = false;
      this.changeDetector.markForCheck();
    });
  }
}