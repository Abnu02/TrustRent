# TrustRent API Documentation

**Version:** 1.0  
**Base URL:** `/api/v1`  
**Authentication:** JWT Bearer Token  
**Roles:** `Tenant`, `Landlord`, `Admin`

---

## 1. Authentication

### 1.1 Register User

**Endpoint**

```http
POST /api/v1/auth/register
```

**Authentication:** None

**Request Body**

```json
{
  "fullName": "Abreham Bekele",
  "email": "abreham@example.com",
  "phoneNumber": "0912345678",
  "password": "Password123!",
  "role": "Landlord"
}
```tt

**Allowed Roles**

```text
Tenant
Landlord
```

**Response — 201 Created**

```json
{
  "message": "Registration successful.",
  "user": {
    "id": "user-id",
    "fullName": "Abreham Bekele",
    "email": "abreham@example.com",
    "phoneNumber": "0912345678",
    "role": "Landlord",
    "isVerified": false
  }
}
```

---

### 1.2 Login

**Endpoint**

```http
POST /api/v1/auth/login
```

**Authentication:** None

**Request Body**

```json
{
  "email": "abreham@example.com",
  "password": "Password123!"
}
```

**Response — 200 OK**

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "user": {
    "id": "user-id",
    "fullName": "Abreham Bekele",
    "email": "abreham@example.com",
    "role": "Landlord",
    "isVerified": false
  }
}
```

---

### 1.3 Logout

**Endpoint**

```http
POST /api/v1/auth/logout
```

**Authentication:** Required

**Request Body**

```json
{}
```

**Response — 200 OK**

```json
{
  "message": "Logout successful."
}
```

---

# 2. Landlord Property Management

All endpoints in this section require:

```http
Authorization: Bearer <access_token>
```

**Required role:** `Landlord`

---

## 2.1 Create Property

**Endpoint**

```http
POST /api/v1/properties
```

**Request Body**

```json
{
  "title": "Modern 2 Bedroom Apartment",
  "description": "Clean and spacious apartment near Bole.",
  "propertyType": "Apartment",
  "rent": 25000,
  "deposit": 50000,
  "location": "Bole, Addis Ababa",
  "bedrooms": 2,
  "bathrooms": 2
}
```

> The landlord does not send `landlordId`. The API gets the landlord ID from the authenticated JWT.

**Response — 201 Created**

```json
{
  "id": "property-id",
  "title": "Modern 2 Bedroom Apartment",
  "status": "Pending",
  "isVerified": false,
  "createdAt": "2026-10-01T10:30:00Z"
}
```

---

## 2.2 Get My Properties

**Endpoint**

```http
GET /api/v1/properties/my
```

**Authentication:** Required

**Role:** `Landlord`

**Response — 200 OK**

```json
[
  {
    "id": "property-id-1",
    "title": "Modern 2 Bedroom Apartment",
    "propertyType": "Apartment",
    "rent": 25000,
    "location": "Bole, Addis Ababa",
    "status": "Pending",
    "isVerified": false
  },
  {
    "id": "property-id-2",
    "title": "Family House",
    "propertyType": "House",
    "rent": 40000,
    "location": "Sarbet, Addis Ababa",
    "status": "Approved",
    "isVerified": true
  }
]
```

---

## 2.3 Update Property

**Endpoint**

```http
PUT /api/v1/properties/{id}
```

**Authentication:** Required

**Role:** `Landlord`

**Request Body**

```json
{
  "title": "Modern 2 Bedroom Apartment",
  "description": "Updated property description.",
  "propertyType": "Apartment",
  "rent": 27000,
  "deposit": 54000,
  "location": "Bole, Addis Ababa",
  "bedrooms": 2,
  "bathrooms": 2
}
```

**Response — 200 OK**

```json
{
  "id": "property-id",
  "message": "Property updated successfully."
}
```

---

# 3. Tenant Property Discovery

These endpoints allow tenants to discover verified properties.

---

## 3.1 Browse Properties

**Endpoint**

```http
GET /api/v1/properties
```

**Authentication:** Optional / Public

### Query Parameters

```text
location
minRent
maxRent
propertyType
bedrooms
bathrooms
```

### Example

```http
GET /api/v1/properties?location=Bole&minRent=10000&maxRent=30000&bedrooms=2
```

**Response — 200 OK**

```json
[
  {
    "id": "property-id",
    "title": "Modern 2 Bedroom Apartment",
    "propertyType": "Apartment",
    "rent": 25000,
    "deposit": 50000,
    "location": "Bole, Addis Ababa",
    "bedrooms": 2,
    "bathrooms": 2,
    "isVerified": true
  }
]
```

### Visibility Rule

Only properties that satisfy all three conditions should appear in the public tenant listing:

```text
Approved
+
Verified
+
Owned by a verified landlord
```

---

# 4. Property Details

## 4.1 Get Property Details

**Endpoint**

```http
GET /api/v1/properties/{id}
```

**Authentication:** Optional

**Response — 200 OK**

```json
{
  "id": "property-id",
  "title": "Modern 2 Bedroom Apartment",
  "description": "Clean and spacious apartment near Bole.",
  "propertyType": "Apartment",
  "rent": 25000,
  "deposit": 50000,
  "location": "Bole, Addis Ababa",
  "bedrooms": 2,
  "bathrooms": 2,
  "status": "Approved",
  "isVerified": true,
  "verifiedAt": "2026-10-01T09:15:00Z",
  "landlord": {
    "id": "landlord-id",
    "fullName": "Abreham Bekele",
    "phoneNumber": "0912345678",
    "email": "abreham@example.com",
    "isVerified": true
  }
}
```

The tenant can use the displayed phone number or email to contact the landlord directly.

> There is no inquiry or chat system in the MVP.

---

# 5. Admin — Landlord Verification

All endpoints require:

```http
Authorization: Bearer <admin_access_token>
```

**Required role:** `Admin`

---

## 5.1 Landlord Account Management

The administration page uses `GET /api/v1/admin/landlords` to list all landlord accounts.
The legacy pending-only endpoint remains available.

## 5.1.1 List All Landlords

```http
GET /api/v1/admin/landlords
```

Each result includes `isVerified` and `isActive` so administrators can manage
approval and account access independently.

## 5.1.2 Get Landlord

```http
GET /api/v1/admin/landlords/{id}
```

## 5.1.3 Create Landlord

```http
POST /api/v1/admin/landlords
Content-Type: application/json
```

```json
{
  "fullName": "Abreham Bekele",
  "email": "abreham@example.com",
  "phoneNumber": "0912345678",
  "password": "StrongPassword1!"
}
```

Newly created landlords start unverified and active.

## 5.1.4 Update Landlord

```http
PUT /api/v1/admin/landlords/{id}
Content-Type: application/json
```

```json
{
  "fullName": "Abreham Bekele",
  "email": "abreham@example.com",
  "phoneNumber": "0912345678"
}
```

## 5.1.5 Deactivate or Reactivate Landlord

`DELETE` deactivates the account rather than deleting its database record, preserving
the landlord's properties. Reactivation uses the `active` endpoint.

```http
DELETE /api/v1/admin/landlords/{id}
PUT /api/v1/admin/landlords/{id}/active
Content-Type: application/json
```

```json
{ "isActive": true }
```

Deactivation returns `204 No Content`. A landlord can also be approved or have
approval revoked using the verification endpoints below.

### List Pending Landlords (legacy)

```http
GET /api/v1/admin/landlords/pending
```

**Response — 200 OK**

```json
[
  {
    "id": "landlord-id",
    "fullName": "Abreham Bekele",
    "email": "abreham@example.com",
    "phoneNumber": "0912345678",
    "isVerified": false
  }
]
```

---

## 5.2 Verify Landlord

**Endpoint**

```http
PUT /api/v1/admin/landlords/{id}/verify
```

**Request Body**

```json
{}
```

**Response — 200 OK**

```json
{
  "message": "Landlord verified successfully.",
  "landlordId": "landlord-id",
  "isVerified": true
}
```

---

## 5.3 Reject Landlord

**Endpoint**

```http
PUT /api/v1/admin/landlords/{id}/reject
```

**Request Body**

```json
{
  "reason": "Identity information could not be verified."
}
```

**Response — 200 OK**

```json
{
  "message": "Landlord verification rejected.",
  "landlordId": "landlord-id",
  "isVerified": false
}
```

---

# 6. Admin — Property Verification

## 6.1 List All Properties

**Endpoint**

```http
GET /api/v1/admin/properties
```

Returns all properties, regardless of `Draft`, `Pending`, `Approved`, or
`Rejected` status, including property details and landlord information. This is
the endpoint used by the admin property-management page; the details dialog uses
the returned property data so unapproved listings remain reviewable.

## 6.2 Get Pending Properties (legacy)

**Endpoint**

```http
GET /api/v1/admin/properties/pending
```

**Response — 200 OK**

```json
[
  {
    "id": "property-id",
    "title": "Modern 2 Bedroom Apartment",
    "location": "Bole, Addis Ababa",
    "rent": 25000,
    "status": "Pending",
    "landlord": {
      "id": "landlord-id",
      "fullName": "Abreham Bekele",
      "isVerified": true
    }
  }
]
```

---

## 6.3 Approve Property

**Endpoint**

```http
PUT /api/v1/admin/properties/{id}/approve
```

**Request Body**

```json
{}
```

**Response — 200 OK**

```json
{
  "message": "Property approved successfully.",
  "propertyId": "property-id",
  "status": "Approved",
  "isVerified": true
}
```

---

## 6.4 Reject Property

**Endpoint**

```http
PUT /api/v1/admin/properties/{id}/reject
```

**Request Body**

```json
{
  "reason": "Property information could not be verified."
}
```

**Response — 200 OK**

```json
{
  "message": "Property rejected.",
  "propertyId": "property-id",
  "status": "Rejected",
  "isVerified": false
}
```

---

# 7. Endpoint Summary

| # | Method | Endpoint | Role |
|---|---|---|---|
| 1 | `POST` | `/api/v1/auth/register` | Public |
| 2 | `POST` | `/api/v1/auth/login` | Public |
| 3 | `POST` | `/api/v1/auth/logout` | Authenticated |
| 4 | `POST` | `/api/v1/properties` | Landlord |
| 5 | `GET` | `/api/v1/properties/my` | Landlord |
| 6 | `PUT` | `/api/v1/properties/{id}` | Landlord |
| 7 | `GET` | `/api/v1/properties` | Public/Tenant |
| 8 | `GET` | `/api/v1/properties/{id}` | Public/Tenant |
| 9 | `GET` | `/api/v1/admin/landlords` | Admin |
| 10 | `GET` | `/api/v1/admin/landlords/{id}` | Admin |
| 11 | `POST` | `/api/v1/admin/landlords` | Admin |
| 12 | `PUT` | `/api/v1/admin/landlords/{id}` | Admin |
| 13 | `DELETE` | `/api/v1/admin/landlords/{id}` (deactivate) | Admin |
| 14 | `PUT` | `/api/v1/admin/landlords/{id}/active` | Admin |
| 15 | `GET` | `/api/v1/admin/landlords/pending` | Admin |
| 16 | `PUT` | `/api/v1/admin/landlords/{id}/verify` | Admin |
| 17 | `PUT` | `/api/v1/admin/landlords/{id}/reject` | Admin |
| 18 | `GET` | `/api/v1/admin/properties` | Admin |
| 19 | `GET` | `/api/v1/admin/properties/pending` (legacy) | Admin |
| 20 | `PUT` | `/api/v1/admin/properties/{id}/approve` | Admin |
| 21 | `PUT` | `/api/v1/admin/properties/{id}/reject` | Admin |

**Total: 21 endpoints**

---

# 8. Verification Flow

```text
LANDLORD
   │
   ├── Register
   │
   ▼
Pending Verification
   │
   ▼
ADMIN
   │
   ├── Verify Landlord
   │
   ▼
VERIFIED LANDLORD
   │
   ├── Create Property
   │
   ▼
Pending Property Verification
   │
   ▼
ADMIN
   │
   ├── Approve Property
   │
   ▼
VERIFIED PROPERTY
   │
   ▼
TENANT
   │
   ├── Browse
   ├── Search
   ├── View Property
   └── See Verified Landlord + Contact
```

---

# 9. MVP Business Rules

1. A user can register as either `Tenant` or `Landlord`.
2. A landlord starts as **unverified**.
3. Only an `Admin` can verify or reject a landlord.
4. An unverified landlord cannot have a property publicly listed as verified.
5. A landlord creates a property with `Pending` status.
6. Only an `Admin` can approve or reject a property.
7. Only approved properties owned by verified landlords are visible to tenants.
8. Tenant property details include the verified landlord's name, phone number, and email.
9. The MVP does not include inquiries, chat, online payments, contracts, AI fraud detection, or advanced maps.
10. `landlordId` is obtained from the authenticated JWT and must not be supplied by the client.
