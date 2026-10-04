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
  imageUrl?: string;
}
export interface PropertyDetails extends Property {
  description: string;
  status: string;
  verifiedAt: string | null;

  landlord: {
    id: string;
    fullName: string;
    phoneNumber: string;
    email: string;
    isVerified: boolean;
  };
}