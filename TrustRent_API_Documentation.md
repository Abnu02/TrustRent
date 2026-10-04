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
```

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
POST /api/v1/landlord/properties
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
GET /api/v1/landlord/properties/my
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
PUT /api/v1/landlord/properties/{id}
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

## 5.1 Get Pending Landlords

**Endpoint**

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
    "verificationStatus": "Pending",
    "createdAt": "2026-10-01T10:30:00Z",
    "reviewedAt": null,
    "reviewNote": null
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
  "id": "landlord-id",
  "fullName": "Abreham Bekele",
  "email": "abreham@example.com",
  "phoneNumber": "0912345678",
  "verificationStatus": "Verified",
  "createdAt": "2026-10-01T10:30:00Z",
  "reviewedAt": "2026-10-02T10:30:00Z",
  "reviewNote": null
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
  "id": "landlord-id",
  "fullName": "Abreham Bekele",
  "email": "abreham@example.com",
  "phoneNumber": "0912345678",
  "verificationStatus": "Rejected",
  "createdAt": "2026-10-01T10:30:00Z",
  "reviewedAt": "2026-10-02T10:30:00Z",
  "reviewNote": "Identity information could not be verified."
}
```

---

# 6. Admin — Audit Log

**Endpoint**

```http
GET /api/v1/admin/audit-log
```

**Authentication:** Required

**Role:** `Admin`

Returns up to 50 most recent property and landlord submission/review events.
`propertyId` is `null` for landlord verification events.

**Response — 200 OK**

```json
[
  {
    "id": "event-id",
    "propertyId": null,
    "action": "Landlord verified",
    "subject": "Abreham Bekele",
    "actorName": "System Administrator",
    "actorEmail": "admin@trustrent.com",
    "status": "Verified",
    "reference": "landlord-id",
    "note": null,
    "occurredAt": "2026-10-02T10:30:00Z"
  }
]
```

---

# 7. Admin — Property Verification

## 6.1 Get Pending Properties

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

## 6.2 Approve Property

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

## 6.3 Reject Property

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

# 8. Endpoint Summary

| # | Method | Endpoint | Role |
|---|---|---|---|
| 1 | `POST` | `/api/v1/auth/register` | Public |
| 2 | `POST` | `/api/v1/auth/login` | Public |
| 3 | `POST` | `/api/v1/auth/logout` | Authenticated |
| 4 | `POST` | `/api/v1/landlord/properties` | Landlord |
| 5 | `GET` | `/api/v1/landlord/properties/my` | Landlord |
| 6 | `PUT` | `/api/v1/landlord/properties/{id}` | Landlord |
| 7 | `GET` | `/api/v1/properties` | Public/Tenant |
| 8 | `GET` | `/api/v1/properties/{id}` | Public/Tenant |
| 9 | `GET` | `/api/v1/admin/landlords/pending` | Admin |
| 10 | `PUT` | `/api/v1/admin/landlords/{id}/verify` | Admin |
| 11 | `PUT` | `/api/v1/admin/landlords/{id}/reject` | Admin |
| 12 | `GET` | `/api/v1/admin/properties/pending` | Admin |
| 13 | `PUT` | `/api/v1/admin/properties/{id}/approve` | Admin |
| 14 | `PUT` | `/api/v1/admin/properties/{id}/reject` | Admin |
| 15 | `GET` | `/api/v1/admin/audit-log` | Admin |

**Total: 15 endpoints**

---

# 9. Verification Flow

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

# 10. MVP Business Rules

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
