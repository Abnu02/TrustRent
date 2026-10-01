export interface LandlordContact {
  id: string;
  fullName: string;
  phoneNumber: string;
  email: string;
  isVerified: boolean;
}

export interface CreatePropertyDto {
  title: string;
  description: string;
  propertyType: string;
  rent: number;
  deposit: number;
  location: string;
  bedrooms: number;
  bathrooms: number;
}

export interface MyProperty {
  id: string;
  title: string;
  rent: number;
  deposit: number;
  location: string;
  propertyType: string;
  bedrooms: number;
  bathrooms: number;
  status: 'Pending' | 'Approved' | 'Rejected';
  isVerified: boolean;
  createdAt: string;
}

export interface PagedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface LandlordStats {
  totalProperties: number;
  pendingCount: number;
  approvedCount: number;
  rejectedCount: number;
  totalMonthlyRevenueETB: number;
}

export interface PropertyCreatedResponse {
  id: string;
  title: string;
  status: string;
  isVerified: boolean;
  createdAt: string;
}

export interface UserProfile {
  id: string;
  fullName: string;
  email: string;
  phoneNumber?: string;
  role: 'Landlord' | 'Tenant' | 'Admin';
  isVerified?: boolean;
}

export interface AuthResponse {
  token?: string;
  user: UserProfile;
}

export interface RegisterLandlordDto {
  fullName: string;
  email: string;
  phoneNumber: string;
  password?: string;
}
