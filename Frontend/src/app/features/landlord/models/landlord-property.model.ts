export interface CreatePropertyRequest {
  title: string;
  description: string;
  propertyType: string;
  rent: number;
  deposit: number;
  location: string;
  bedrooms: number;
  bathrooms: number;
}

export interface CreatePropertyResponse {
  id: string;
  title: string;
  status: string;
  isVerified: boolean;
  createdAt: string;
}

export interface LandlordProperty {
  id: string;
  title: string;
  propertyType: string;
  rent: number;
  location: string;
  status: 'Pending' | 'Approved' | 'Rejected';
  isVerified: boolean;
  imageUrl?: string;
}