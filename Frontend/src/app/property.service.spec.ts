import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { PropertyDraft, PropertyService } from './property.service';

describe('PropertyService', () => {
  let service: PropertyService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(PropertyService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('sends supported paging, filters, search, and sort values to my-properties', () => {
    service.getMy({
      page: 2,
      pageSize: 10,
      status: 'Pending',
      propertyType: 'Apartment',
      search: 'Bole',
      sortBy: 'rent',
      sortDirection: 'desc',
    }).subscribe();

    const request = http.expectOne(request => request.url === '/api/v1/landlord/properties/my');
    expect(request.request.method).toBe('GET');
    expect(request.request.params.get('page')).toBe('2');
    expect(request.request.params.get('pageSize')).toBe('10');
    expect(request.request.params.get('status')).toBe('Pending');
    expect(request.request.params.get('propertyType')).toBe('Apartment');
    expect(request.request.params.get('search')).toBe('Bole');
    expect(request.request.params.get('sortBy')).toBe('rent');
    expect(request.request.params.get('sortDirection')).toBe('desc');
    request.flush({ items: [], page: 2, pageSize: 10, totalCount: 0, totalPages: 0 });
  });

  it('posts only fields accepted by create-property', () => {
    const draft: PropertyDraft = {
      title: 'Apartment',
      description: 'Two bedrooms',
      propertyType: 'Apartment',
      rent: 25000,
      deposit: 50000,
      location: 'Bole, Addis Ababa',
      bedrooms: 2,
      bathrooms: 2,
    };
    service.create(draft).subscribe();

    const request = http.expectOne('/api/v1/landlord/properties');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(draft);
    expect(request.request.body.status).toBeUndefined();
    expect(request.request.body.isVerified).toBeUndefined();
    request.flush({ ...draft, id: 'property-id', status: 'Pending', isVerified: false });
  });

  it('puts supported property fields to the selected property endpoint', () => {
    const draft: PropertyDraft = {
      title: 'Apartment',
      description: 'Two bedrooms',
      propertyType: 'Apartment',
      rent: 25000,
      deposit: 50000,
      location: 'Bole, Addis Ababa',
      bedrooms: 2,
      bathrooms: 2,
    };
    service.update('property-id', draft).subscribe();

    const request = http.expectOne('/api/v1/landlord/properties/property-id');
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual(draft);
    request.flush({ ...draft, id: 'property-id', status: 'Approved', isVerified: true });
  });
});