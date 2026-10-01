import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { throwError } from 'rxjs';
import { PropertyService } from '../../property.service';
import { PropertyListComponent } from './property-list.component';

describe('PropertyListComponent', () => {
  it('shows a recoverable error instead of leaving the loading skeleton active', async () => {
    await TestBed.configureTestingModule({
      imports: [PropertyListComponent],
      providers: [
        provideRouter([]),
        { provide: PropertyService, useValue: { getMy: () => throwError(() => new HttpErrorResponse({ status: 0 })) } },
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(PropertyListComponent);
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    expect(fixture.componentInstance.loading).toBe(false);
    expect(fixture.componentInstance.errorMessage).toContain('could not be reached');
    expect(fixture.nativeElement.querySelector('.list-error')).not.toBeNull();
    expect(fixture.nativeElement.querySelector('.list-skeleton')).toBeNull();
  });
});