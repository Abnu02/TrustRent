import { Component, signal, computed, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LandlordService } from './services/landlord.service';
import { AuthService } from './services/auth.service';
import { CreatePropertyDto, MyProperty, PagedResponse, LandlordStats } from './models/property.model';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit {
  protected readonly authService = inject(AuthService);
  protected readonly landlordService = inject(LandlordService);
  private http = inject(HttpClient);

  // Navigation & Role states
  activeNav = signal<string>('landlord-portal');
  activeRole = signal<'Tenant' | 'Landlord' | 'Auditor'>('Landlord');

  // Modal & Drawer visibility signals
  isCreateModalOpen = signal<boolean>(false);
  isDrawerOpen = signal<boolean>(false);
  isRegisterModalOpen = signal<boolean>(false);

  // Stepper state in Create Property Modal (1: Details, 2: Pricing, 3: Deed & Ownership, 4: Audit)
  currentStep = signal<number>(3);

  // Loading & Toast signals
  isLoading = signal<boolean>(false);
  toastMessage = signal<string | null>(null);
  toastType = signal<'success' | 'error'>('success');

  // ==========================================
  // PAGINATION, FILTERING & SEARCH SIGNALS (TMS Modules 6 & 8)
  // ==========================================
  page = signal<number>(1);
  pageSize = signal<number>(6);
  statusFilter = signal<string>('All');
  searchTerm = signal<string>('');
  sortBy = signal<string>('createdAt');
  sortDescending = signal<boolean>(true);

  // Paged response & Stats state
  pagedData = signal<PagedResponse<MyProperty> | null>(null);
  stats = signal<LandlordStats | null>(null);

  // Computed signals
  properties = computed(() => this.pagedData()?.items ?? []);
  totalCount = computed(() => this.pagedData()?.totalCount ?? 0);
  totalPages = computed(() => this.pagedData()?.totalPages ?? 1);
  hasPrev = computed(() => this.pagedData()?.hasPreviousPage ?? false);
  hasNext = computed(() => this.pagedData()?.hasNextPage ?? false);

  // ==========================================
  // FORM MODELS (Ethiopian Context & Strict Verifications)
  // ==========================================
  newProperty: CreatePropertyDto = {
    title: 'Luxury 2-Bedroom Condo in Bole Medhanialem',
    description: 'Modern luxury condominium with standby generator, 24/7 backup water tanker, and high security.',
    propertyType: 'Condominium',
    rent: 28000,
    deposit: 56000,
    location: 'Bole, Addis Ababa',
    bedrooms: 2,
    bathrooms: 2
  };

  // Form Specs
  streetAddress = 'Cameroon St, Near Edna Mall';
  unitApt = 'Unit 402, 4th Floor';
  city = 'Addis Ababa';
  subcity = 'Bole';
  neighborhood = 'Bole Medhanialem / Brass';
  squareFootage = 1150;
  yearBuilt = 2022;
  leaseTerm = '12 Months';
  availableDate = 'Nov 1, 2026';
  legalAttest = signal<boolean>(true);

  // Registration Form Model
  registerForm = {
    fullName: 'Abreham Bekele',
    email: 'abreham@example.com',
    phoneNumber: '0912345678',
    password: 'password123'
  };

  // Real estate photography collection
  readonly propertyImages: string[] = [
    'https://lh3.googleusercontent.com/aida-public/AB6AXuC4GkmJp0yFpbR4rjoIj7oS0S_Yipj6HdRYF2rKLT0aOYQa-2y8n1jo2isQg1N7GgkAz19DC6eNeRpxPA034jSX2Wxp0GS-1at7OGONUqSQAJ5Nkvy65T3iWw9DM27lCyziprCbBG-HxzX5vWPSVuAAZwXWyrpN9loma9cGwki4xjvWEtcXdiKv5qauS_jHu0biwXnS8lajCmlTsX0FX4tFTeHcUofcT_fjZv2ICndHPzbV-RrUa7a6dQ',
    'https://lh3.googleusercontent.com/aida-public/AB6AXuDh6oks2Ck2JK_ab1J_jSw2QqzNK4Y3G6B6bhteFqOd0Lby7pkv3jDzDO2hEYuEaAS9NUKmNchOz_5ZWNMRwNzeow_B0vGp-X0LOit5KmyoH5ib-hHVMwBXwvJgje9PGgiG5EZUhVmfUBhRQiFlLgmtDQZ2CnAaM-2Piya1xqoiLDRyuZYVaPgP2hEzhJ6PYODh6ctmUjUEtU3XsFOjqPOCoCESzfeObbXN-0zO5Rs_06_Qi2AJKROfkQ',
    'https://lh3.googleusercontent.com/aida-public/AB6AXuA_4sZ4g_XM-fZg6un6WmAnQelKSPTkUMiXG1hgyYN7poVSKcLIUgLK5rmNA1G7vBa0AOM0xPmBbO5_0OfsGOOaRwZ9D4Vr_e-9HwhZrZjXVVxIXwCOwuZMotoclMLp4DwJ8e-wFgjiqbZzspTB7RhPzCIaOFCUzpqm4s8PDN_EZlno_PW8AbYkzb6QkuQyYBy6yIbBZJn2ZiI2EC9thLsJvCcpok1aJAIPnVdF91OeKMnISEZzBqXUCg',
    'https://lh3.googleusercontent.com/aida-public/AB6AXuDRxoAkRBCgv45Al5xvA_LWlhQusmTbdovqa1MSWRcgUAZIqup0rSTrW5f70Hy3WtSvuCHB8h3Np-QyuggTkltiEtuhU88CQKQaeuX6MRKSg9PH32hcxHCrkJAgAAlte3h-JtMR51mcdM9cxkD9wuEJd-JNcxZG-_YHuDI1MQxg_qwPXTScAh7htlssefpN9BEqkiCyT7KrtIb52eao6r5ghHa9c68W00BoAsciunKlsSmVogeM_1s9KA',
    'https://lh3.googleusercontent.com/aida-public/AB6AXuCYsNd7lo-QyEpcF0-cNzDoK2wKue7YYe8ZCFRnVUE9PhZiekHPd4msj12WncNjJTXnjNKnz2ojYeIqxIlCWLJqsYBdtD4iFRJ693A-zUM5BjRiWiMTXIoU37h5tXy1wq1FpE-ym2sQIGUzXI5Yt2CyQddD80bqqIfxMexns8-fia1_jQavNgvm8XQnz3K3U5Z80cl7SOMmk2YCvv9L-yMbiKME-pDKB0dwrSGPUfs8dJ1xqg0UtpVPHw',
    'https://lh3.googleusercontent.com/aida-public/AB6AXuC-G2lKeenGn0ON6EtT31ve5DIv-oyFTd0-Je8JNWRSwArcmCGGAEWWbmXxa4Y_ym0z6q5LMUad0xEAt9sW_u6llrHCwmLJGY4etYwan3qR4Mi_1CXULUfblnd52fyFcdhEU5g2spV7GKFosyaU9crGL0zKUi0GmoPEoouTF89CTl9ld6tiDxqiPy7SXg7ln0iJ6q-8yCfxEyfnrXkh5yb7GyHOI3ZRk2JVFTPJbTPIBlopB6AWuZD-fQ'
  ];

  ngOnInit() {
    this.loadProperties();
    this.loadStats();
  }

  getPropertyImage(index: number): string {
    return this.propertyImages[index % this.propertyImages.length];
  }

  getMockViews(index: number): number {
    return 180 + (index * 73) % 240;
  }

  getMockInquiries(index: number): number {
    return 6 + (index * 4) % 19;
  }

  showToast(message: string, type: 'success' | 'error' = 'success') {
    this.toastMessage.set(message);
    this.toastType.set(type);
    setTimeout(() => this.toastMessage.set(null), 5000);
  }

  // ==========================================
  // DATA FETCHING & PAGINATION (TMS Pattern)
  // ==========================================
  loadProperties() {
    this.isLoading.set(true);
    this.landlordService.getMyPropertiesPaged(
      this.page(),
      this.pageSize(),
      this.statusFilter(),
      this.searchTerm(),
      this.sortBy(),
      this.sortDescending()
    ).subscribe({
      next: (response) => {
        this.pagedData.set(response);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Failed to load paginated properties:', err);
        this.isLoading.set(false);
      }
    });
  }

  loadStats() {
    this.landlordService.getLandlordStats().subscribe({
      next: (data) => this.stats.set(data),
      error: (err) => console.error('Failed to load stats:', err)
    });
  }

  onPageChange(newPage: number) {
    if (newPage >= 1 && newPage <= this.totalPages()) {
      this.page.set(newPage);
      this.loadProperties();
    }
  }

  onStatusFilterChange(status: string) {
    this.statusFilter.set(status);
    this.page.set(1);
    this.loadProperties();
  }

  onSearchChange() {
    this.page.set(1);
    this.loadProperties();
  }

  onSortChange(sortField: string) {
    if (this.sortBy() === sortField) {
      this.sortDescending.set(!this.sortDescending());
    } else {
      this.sortBy.set(sortField);
      this.sortDescending.set(true);
    }
    this.page.set(1);
    this.loadProperties();
  }

  // ==========================================
  // MODAL & DRAWER TRIGGERS
  // ==========================================
  openCreateModal() {
    this.currentStep.set(3);
    this.isCreateModalOpen.set(true);
  }

  closeCreateModal() {
    this.isCreateModalOpen.set(false);
  }

  openDrawer() {
    this.isDrawerOpen.set(true);
  }

  closeDrawer() {
    this.isDrawerOpen.set(false);
  }

  openRegisterModal() {
    this.isRegisterModalOpen.set(true);
  }

  closeRegisterModal() {
    this.isRegisterModalOpen.set(false);
  }

  setStep(step: number) {
    this.currentStep.set(step);
  }

  onSubcitySelect(event: Event) {
    const select = event.target as HTMLSelectElement;
    if (select && select.value) {
      this.newProperty.location = select.value;
    }
  }

  // ==========================================
  // LANDLORD WORKFLOW: CREATE & SUBMIT PROPERTY
  // Sends STRICTLY the 8 fields defined in backend CreatePropertyRequest:
  // (Title, Description, PropertyType, Rent, Deposit, Location, Bedrooms, Bathrooms)
  // ==========================================
  onSubmitProperty() {
    if (!this.newProperty.title || !this.newProperty.title.trim()) {
      this.showToast('Please provide a property title.', 'error');
      return;
    }

    if (!this.newProperty.description || !this.newProperty.description.trim()) {
      this.showToast('Please provide a property description.', 'error');
      return;
    }

    if (!this.newProperty.location || !this.newProperty.location.trim()) {
      this.showToast('Please provide a location (e.g. Bole, Addis Ababa).', 'error');
      return;
    }

    if (!this.newProperty.rent || this.newProperty.rent <= 0) {
      this.showToast('Monthly rent must be greater than 0 ETB.', 'error');
      return;
    }

    if (this.newProperty.deposit < 0) {
      this.showToast('Deposit amount cannot be negative.', 'error');
      return;
    }

    if (!this.legalAttest()) {
      this.showToast('You must certify ownership & legal deed credentials before submitting.', 'error');
      return;
    }

    // STRICTLY send ONLY the 8 fields defined in the backend CreatePropertyRequest record:
    const payload: CreatePropertyDto = {
      title: this.newProperty.title.trim(),
      description: this.newProperty.description.trim(),
      propertyType: this.newProperty.propertyType || 'Apartment',
      rent: Number(this.newProperty.rent),
      deposit: Number(this.newProperty.deposit),
      location: this.newProperty.location.trim(),
      bedrooms: Number(this.newProperty.bedrooms) > 0 ? Number(this.newProperty.bedrooms) : 1,
      bathrooms: Number(this.newProperty.bathrooms) > 0 ? Number(this.newProperty.bathrooms) : 1
    };

    this.isLoading.set(true);
    this.landlordService.createProperty(payload).subscribe({
      next: (response) => {
        this.isLoading.set(false);
        this.closeCreateModal();
        this.showToast(`Property "${response.title}" submitted to backend! Status: PENDING Auditor Review.`, 'success');
        
        // Reset form
        this.newProperty = {
          title: '',
          description: '',
          propertyType: 'Condominium',
          rent: 25000,
          deposit: 50000,
          location: 'Bole, Addis Ababa',
          bedrooms: 2,
          bathrooms: 2
        };

        // Reset to first page & refresh properties & KPI counters
        this.statusFilter.set('All');
        this.page.set(1);
        this.loadProperties();
        this.loadStats();
      },
      error: (err) => {
        this.isLoading.set(false);
        this.showToast(err.error?.detail || err.error?.message || 'Failed to submit property.', 'error');
      }
    });
  }

  // ==========================================
  // LANDLORD ONBOARDING: REGISTER
  // ==========================================
  onRegisterLandlord() {
    if (!this.registerForm.fullName || !this.registerForm.email || !this.registerForm.phoneNumber) {
      this.showToast('Please fill out all registration fields.', 'error');
      return;
    }

    this.isLoading.set(true);
    this.landlordService.registerLandlord({
      fullName: this.registerForm.fullName,
      email: this.registerForm.email,
      phoneNumber: this.registerForm.phoneNumber,
      password: this.registerForm.password
    }).subscribe({
      next: (res) => {
        this.isLoading.set(false);
        this.authService.currentUser.set({
          id: res.id,
          fullName: res.fullName,
          email: res.email,
          phoneNumber: res.phoneNumber,
          role: 'Landlord',
          isVerified: res.isVerified
        });
        this.closeRegisterModal();
        this.showToast(`Welcome ${res.fullName}! Landlord profile active.`, 'success');
        this.loadProperties();
        this.loadStats();
      },
      error: (err) => {
        this.isLoading.set(false);
        this.showToast(err.error?.detail || err.error?.message || 'Registration failed.', 'error');
      }
    });
  }

  // Demo Helper: Simulates Admin approval so judges see status flip live!
  simulateAdminApprove(propertyId: string) {
    this.isLoading.set(true);
    this.http.put(`http://localhost:5151/api/v1/admin/properties/${propertyId}/approve`, {}).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.showToast('Auditor verification complete! Listing is now APPROVED & LIVE on TrustRent.', 'success');
        this.loadProperties();
        this.loadStats();
      },
      error: () => {
        this.isLoading.set(false);
        this.showToast('Failed to approve property.', 'error');
      }
    });
  }
}
