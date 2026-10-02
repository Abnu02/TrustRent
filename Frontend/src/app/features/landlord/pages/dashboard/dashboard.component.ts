import { Component, computed, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DecimalPipe } from '@angular/common';
interface LandlordProperty {
  id: string;
  title: string;
  propertyType: string;
  rent: number;
  location: string;
  status: 'Pending' | 'Approved' | 'Rejected';
  isVerified: boolean;
  imageUrl?: string;
}

@Component({
  selector: 'app-landlord-dashboard',
  standalone: true,
  imports: [
    RouterLink,
    DecimalPipe
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent {

  properties = signal<LandlordProperty[]>([
    {
      id: 'property-id-1',
      title: 'Modern 2 Bedroom Apartment',
      propertyType: 'Apartment',
      rent: 25000,
      location: 'Bole, Addis Ababa',
      status: 'Pending',
      isVerified: false,
      imageUrl:
        'https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?auto=format&fit=crop&w=600&q=80'
    },
    {
      id: 'property-id-2',
      title: 'Family House',
      propertyType: 'House',
      rent: 40000,
      location: 'Sarbet, Addis Ababa',
      status: 'Approved',
      isVerified: true,
      imageUrl:
        'https://images.unsplash.com/photo-1564013799919-ab600027ffc6?auto=format&fit=crop&w=600&q=80'
    },
    {
      id: 'property-id-3',
      title: 'Modern Studio Apartment',
      propertyType: 'Apartment',
      rent: 18000,
      location: 'Kazanchis, Addis Ababa',
      status: 'Approved',
      isVerified: true,
      imageUrl:
        'https://images.unsplash.com/photo-1505693416388-ac5ce068fe85?auto=format&fit=crop&w=600&q=80'
    },
    {
      id: 'property-id-4',
      title: 'Spacious Family Home',
      propertyType: 'House',
      rent: 35000,
      location: 'CMC, Addis Ababa',
      status: 'Rejected',
      isVerified: false,
      imageUrl:
        'https://images.unsplash.com/photo-1570129477492-45c003edd2be?auto=format&fit=crop&w=600&q=80'
    }
  ]);

  totalProperties = computed(() =>
    this.properties().length
  );

  pendingProperties = computed(() =>
    this.properties()
      .filter(property => property.status === 'Pending')
      .length
  );

  approvedProperties = computed(() =>
    this.properties()
      .filter(property => property.status === 'Approved')
      .length
  );

  rejectedProperties = computed(() =>
    this.properties()
      .filter(property => property.status === 'Rejected')
      .length
  );

  recentProperties = computed(() =>
    this.properties().slice(0, 3)
  );

  getStatusClass(status: string): string {
    switch (status) {
      case 'Approved':
        return 'status-approved';

      case 'Rejected':
        return 'status-rejected';

      default:
        return 'status-pending';
    }
  }
}