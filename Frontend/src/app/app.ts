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

  // Active view tab: 'create' | 'my-properties' | 'register'
  activeTab = signal<'create' | 'my-properties' | 'register'>('my-properties');

  // Loading state
  isLoading = signal<boolean>(false);

  // Notification Toast
  toastMessage = signal<string | null>(null);
  toastType = signal<'success' | 'error'>('success');

  // ==========================================
  // PAGINATION, FILTERING & SEARCH SIGNALS (Module 6 & 8)
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

  // Form Models
  newProperty: CreatePropertyDto = {
    title: 'Modern 2 Bedroom Apartment',
    description: 'Clean and spacious apartment near Bole Edna Mall with 24/7 water backup reservoir.',
    propertyType: 'Apartment',
    rent: 25000,
    deposit: 50000,
    location: 'Bole, Addis Ababa',
    bedrooms: 2,
    bathrooms: 2
  };

  registerForm = {
    fullName: 'Abreham Bekele',
    email: 'abreham@example.com',
    phoneNumber: '0912345678',
    password: 'password123'
  };

  ngOnInit() {
    this.loadProperties();
    this.loadStats();
  }

  showToast(message: string, type: 'success' | 'error' = 'success') {
    this.toastMessage.set(message);
    this.toastType.set(type);
    setTimeout(() => this.toastMessage.set(null), 4500);
  }

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

  // ==========================================
  // PAGINATION CONTROLS
  // ==========================================
  onPageChange(newPage: number) {
    if (newPage >= 1 && newPage <= this.totalPages()) {
      this.page.set(newPage);
      this.loadProperties();
    }
  }

  onStatusFilterChange(status: string) {
    this.statusFilter.set(status);
    this.page.set(1); // Reset to first page
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
  // LANDLORD WORKFLOW ACTIONS
  // ==========================================
  onSubmitProperty() {
    if (!this.newProperty.title || !this.newProperty.location || this.newProperty.rent <= 0) {
      this.showToast('Please fill all required fields with valid values.', 'error');
      return;
    }

    this.isLoading.set(true);
    this.landlordService.createProperty(this.newProperty).subscribe({
      next: (response) => {
        this.isLoading.set(false);
        this.showToast(`Property "${response.title}" submitted! Status: PENDING Admin review.`, 'success');
        
        // Refresh list & stats, then switch to My Listings
        this.page.set(1);
        this.loadProperties();
        this.loadStats();
        this.activeTab.set('my-properties');
      },
      error: (err) => {
        this.isLoading.set(false);
        this.showToast(err.error?.detail || err.error?.message || 'Failed to submit property.', 'error');
      }
    });
  }

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
        this.showToast(`Landlord ${res.fullName} registered! Proceed to create your property.`, 'success');
        this.loadProperties();
        this.loadStats();
        this.activeTab.set('create');
      },
      error: (err) => {
        this.isLoading.set(false);
        this.showToast(err.error?.detail || err.error?.message || 'Registration failed.', 'error');
      }
    });
  }

  // Demo Helper: Simulates Admin approval so judges see status flip live!
  simulateAdminApprove(propertyId: string) {
    this.http.put(`http://localhost:5151/api/v1/admin/properties/${propertyId}/approve`, {}).subscribe({
      next: () => {
        this.showToast('Admin approved listing! Status is now VERIFIED & LIVE.', 'success');
        this.loadProperties();
        this.loadStats();
      },
      error: () => {
        this.showToast('Failed to approve property.', 'error');
      }
    });
  }
}
