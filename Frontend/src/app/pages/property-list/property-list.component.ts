import { ChangeDetectorRef, Component, DestroyRef, OnInit, inject } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { EMPTY, Subject, catchError, debounceTime, finalize, merge, startWith, switchMap, tap } from 'rxjs';
import { toApiErrorMessage } from '../../api-error';
import { Property, PropertyQuery, PropertyService, PropertyStatus, PropertyType } from '../../property.service';
import { PropertyStateComponent } from '../../shared/property-state.component';

type SortOption = 'title-asc' | 'title-desc' | 'location-asc' | 'location-desc' | 'rent-asc' | 'rent-desc';

@Component({
  selector: 'app-property-list',
  standalone: true,
  imports: [DecimalPipe, ReactiveFormsModule, RouterLink, PropertyStateComponent],
  templateUrl: './property-list.component.html',
  styleUrl: './property-list.component.scss',
})
export class PropertyListComponent implements OnInit {
  private readonly propertyService = inject(PropertyService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly changeDetector = inject(ChangeDetectorRef);
  private readonly reload$ = new Subject<void>();

  readonly filters = new FormGroup({
    search: new FormControl('', { nonNullable: true }),
    status: new FormControl<PropertyStatus | ''>('', { nonNullable: true }),
    propertyType: new FormControl<PropertyType | ''>('', { nonNullable: true }),
    sort: new FormControl<SortOption>('title-asc', { nonNullable: true }),
  });
  properties: Property[] = [];
  page = 1;
  readonly pageSize = 10;
  totalCount = 0;
  totalPages = 1;
  loading = true;
  errorMessage = '';

  get firstItem(): number { return this.totalCount === 0 ? 0 : (this.page - 1) * this.pageSize + 1; }
  get lastItem(): number { return Math.min(this.page * this.pageSize, this.totalCount); }

  ngOnInit(): void {
    merge(
      this.filters.valueChanges.pipe(debounceTime(250), tap(() => this.page = 1)),
      this.reload$.pipe(startWith(undefined)),
    ).pipe(
      tap(() => { this.loading = true; this.errorMessage = ''; }),
      switchMap(() => this.propertyService.getMy(this.createQuery()).pipe(
        catchError(error => {
          this.errorMessage = toApiErrorMessage(error).message;
          this.changeDetector.markForCheck();
          return EMPTY;
        }),
        finalize(() => {
          this.loading = false;
          this.changeDetector.markForCheck();
        }),
      )),
      takeUntilDestroyed(this.destroyRef),
    ).subscribe(response => {
      this.properties = response.items;
      this.page = response.page;
      this.totalCount = response.totalCount;
      this.totalPages = Math.max(response.totalPages, 1);
      this.changeDetector.markForCheck();
    });
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages || page === this.page) return;
    this.page = page;
    this.reload$.next();
  }

  retry(): void {
    this.reload$.next();
  }

  private createQuery(): PropertyQuery {
    const value = this.filters.getRawValue();
    const [sortKey, sortDirectionValue] = value.sort.split('-');
    const sortBy: PropertyQuery['sortBy'] = sortKey === 'rent' || sortKey === 'location' ? sortKey : 'title';
    const sortDirection: PropertyQuery['sortDirection'] = sortDirectionValue === 'desc' ? 'desc' : 'asc';
    return {
      page: this.page,
      pageSize: this.pageSize,
      search: value.search.trim() || undefined,
      status: value.status || undefined,
      propertyType: value.propertyType || undefined,
      sortBy,
      sortDirection,
    };
  }
}