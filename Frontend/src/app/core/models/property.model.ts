export interface Property {
  id: string;
  title: string;
  propertyType: string;
  rent: number;
  deposit: number;
  location: string;
  bedrooms: number;
  bathrooms: number;
  isVerified: boolean;

  // Frontend-only field for demo images.
  // Backend documentation does not currently provide imageUrl.
  imageUrl?: string;
}
export interface PropertyDetails extends Property {
  description: string;
  status: string;
  verifiedAt: string;

  landlord: {
    id: string;
    fullName: string;
    phoneNumber: string;
    email: string;
    isVerified: boolean;
  };
}