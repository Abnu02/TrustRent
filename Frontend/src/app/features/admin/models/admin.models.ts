export interface AdminLandlord {
  id: string;
  fullName: string;
  email: string;
  phoneNumber: string;
  isVerified: boolean;
  isActive: boolean;
}

export interface CreateLandlordRequest {
  fullName: string;
  email: string;
  phoneNumber: string;
  password: string;
}

export interface UpdateLandlordRequest {
  fullName: string;
  email: string;
  phoneNumber: string;
}

export interface AdminProperty {
  id: string;
  title: string;
  description: string;
  propertyType: string;
  location: string;
  rent: number;
  deposit: number;
  bedrooms: number;
  bathrooms: number;
  status: string;
  isVerified: boolean;
  createdAt: string;
  imageUrl?: string | null;
  landlord: {
    id: string;
    fullName: string;
    email: string;
    phoneNumber: string;
    isVerified: boolean;
  };
}

export interface RejectRequest {
  reason: string;
}