import { Injectable, signal, computed } from '@angular/core';
import { Property } from '../../../core/models/property.model';

@Injectable({
  providedIn: 'root'
})
export class PropertiesStore {

  private readonly allProperties = signal<Property[]>([
    {
      id: 'property-001',
      title: 'Modern 2 Bedroom Apartment',
      propertyType: 'Apartment',
      rent: 25000,
      deposit: 50000,
      location: 'Bole, Addis Ababa',
      bedrooms: 2,
      bathrooms: 2,
      isVerified: true,
      imageUrl:
        'https://images.unsplash.com/photo-1600607687939-ce8a6c25118c?auto=format&fit=crop&w=900&q=80'
    },

    {
      id: 'property-002',
      title: 'Bright Family Apartment',
      propertyType: 'Apartment',
      rent: 32000,
      deposit: 64000,
      location: 'Sarbet, Addis Ababa',
      bedrooms: 3,
      bathrooms: 2,
      isVerified: true,
      imageUrl:
        'https://images.unsplash.com/photo-1600566753190-17f0baa2a6c3?auto=format&fit=crop&w=900&q=80'
    },

    {
      id: 'property-003',
      title: 'Affordable Studio Apartment',
      propertyType: 'Apartment',
      rent: 15000,
      deposit: 30000,
      location: 'Kazanchis, Addis Ababa',
      bedrooms: 1,
      bathrooms: 1,
      isVerified: true,
      imageUrl:
        'https://images.unsplash.com/photo-1502672260266-1c1ef2d93688?auto=format&fit=crop&w=900&q=80'
    },

    {
      id: 'property-004',
      title: 'Luxury 3 Bedroom Residence',
      propertyType: 'Apartment',
      rent: 45000,
      deposit: 90000,
      location: 'Old Airport, Addis Ababa',
      bedrooms: 3,
      bathrooms: 3,
      isVerified: true,
      imageUrl:
        'https://images.unsplash.com/photo-1600607687920-4e2a09cf159d?auto=format&fit=crop&w=900&q=80'
    },

    {
      id: 'property-005',
      title: 'Modern One Bedroom Home',
      propertyType: 'Apartment',
      rent: 18000,
      deposit: 36000,
      location: 'Gerji, Addis Ababa',
      bedrooms: 1,
      bathrooms: 1,
      isVerified: true,
      imageUrl:
        'https://images.unsplash.com/photo-1600585154340-be6161a56a0c?auto=format&fit=crop&w=900&q=80'
    },

    {
      id: 'property-006',
      title: 'Spacious Family House',
      propertyType: 'House',
      rent: 55000,
      deposit: 110000,
      location: 'CMC, Addis Ababa',
      bedrooms: 4,
      bathrooms: 3,
      isVerified: true,
      imageUrl:
        'https://images.unsplash.com/photo-1600047509807-ba8f99d2cdde?auto=format&fit=crop&w=900&q=80'
    },

    {
      id: 'property-007',
      title: 'Cozy Two Bedroom Apartment',
      propertyType: 'Apartment',
      rent: 22000,
      deposit: 44000,
      location: 'Megenagna, Addis Ababa',
      bedrooms: 2,
      bathrooms: 2,
      isVerified: true,
      imageUrl:
        'https://images.unsplash.com/photo-1600210492486-724fe5c67fb0?auto=format&fit=crop&w=900&q=80'
    },

    {
      id: 'property-008',
      title: 'Elegant 3 Bedroom Condo',
      propertyType: 'Condo',
      rent: 38000,
      deposit: 76000,
      location: 'Ayat, Addis Ababa',
      bedrooms: 3,
      bathrooms: 2,
      isVerified: true,
      imageUrl:
        'https://images.unsplash.com/photo-1600566753086-00f18fb6b3ea?auto=format&fit=crop&w=900&q=80'
    },

    {
      id: 'property-009',
      title: 'Comfortable Studio Home',
      propertyType: 'Studio',
      rent: 12000,
      deposit: 24000,
      location: 'Piassa, Addis Ababa',
      bedrooms: 1,
      bathrooms: 1,
      isVerified: true,
      imageUrl:
        'https://images.unsplash.com/photo-1505693416388-ac5ce068fe85?auto=format&fit=crop&w=900&q=80'
    },

    {
      id: 'property-010',
      title: 'Premium Bole Residence',
      propertyType: 'Apartment',
      rent: 48000,
      deposit: 96000,
      location: 'Bole Atlas, Addis Ababa',
      bedrooms: 3,
      bathrooms: 3,
      isVerified: true,
      imageUrl:
        'https://images.unsplash.com/photo-1600607688969-a5bfcd646154?auto=format&fit=crop&w=900&q=80'
    }
  ]);

  readonly properties = computed(() =>
    this.allProperties().filter(property => property.isVerified)
  );

  getPropertyById(id: string): Property | undefined {
    return this.properties().find(property => property.id === id);
  }
}