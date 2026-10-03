export type UserRole = 'Tenant' | 'Landlord' | 'Admin';

export interface RegisterRequest {
  fullName: string;
  email: string;
  phoneNumber: string;
  password: string;
  role: 'Tenant' | 'Landlord';
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface UserResponse {
  id: string;
  fullName: string;
  email: string;
  phoneNumber?: string;
  role: UserRole;
  isVerified: boolean;
}

export interface LoginResponse extends UserResponse {
  accessToken: string;
}