# API Examples

Comprehensive examples for using the User Service API.

## Base URL

```
Development: http://localhost:5000/api
Production: https://your-domain.com/api
```

## Users API

### Create a New User

**Request:**
```bash
POST /api/users
Content-Type: application/json

{
  "email": "john.doe@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "bio": "Software developer passionate about clean code",
  "location": "San Francisco, CA",
  "avatarUrl": "https://example.com/avatar.jpg",
  "isActive": true,
  "isAdmin": false
}
```

**Response (201 Created):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "email": "john.doe@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "bio": "Software developer passionate about clean code",
  "location": "San Francisco, CA",
  "avatarUrl": "https://example.com/avatar.jpg",
  "isActive": true,
  "isAdmin": false,
  "lastLoginAt": null,
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-15T10:30:00Z"
}
```

### Get User by ID

**Request:**
```bash
GET /api/users/550e8400-e29b-41d4-a716-446655440000
```

**Response (200 OK):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "email": "john.doe@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "bio": "Software developer passionate about clean code",
  "location": "San Francisco, CA",
  "avatarUrl": "https://example.com/avatar.jpg",
  "isActive": true,
  "isAdmin": false,
  "lastLoginAt": "2024-01-15T14:30:00Z",
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-15T14:30:00Z"
}
```

### Get User by Email

**Request:**
```bash
GET /api/users/by-email/john.doe@example.com
```

**Response (200 OK):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "email": "john.doe@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "bio": "Software developer passionate about clean code",
  "location": "San Francisco, CA",
  "avatarUrl": "https://example.com/avatar.jpg",
  "isActive": true,
  "isAdmin": false,
  "lastLoginAt": "2024-01-15T14:30:00Z",
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-15T14:30:00Z"
}
```

### Get All Users

**Request:**
```bash
GET /api/users
```

**Response (200 OK):**
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "email": "john.doe@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "isActive": true,
    "isAdmin": false,
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-15T14:30:00Z"
  },
  {
    "id": "660e8400-e29b-41d4-a716-446655440001",
    "email": "jane.smith@example.com",
    "firstName": "Jane",
    "lastName": "Smith",
    "isActive": true,
    "isAdmin": true,
    "createdAt": "2024-01-14T09:00:00Z",
    "updatedAt": "2024-01-15T11:20:00Z"
  }
]
```

### Get Active Users

**Request:**
```bash
GET /api/users/active
```

**Response (200 OK):**
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "email": "john.doe@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "isActive": true,
    "isAdmin": false,
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-15T14:30:00Z"
  }
]
```

### Get Admin Users

**Request:**
```bash
GET /api/users/admins
```

**Response (200 OK):**
```json
[
  {
    "id": "660e8400-e29b-41d4-a716-446655440001",
    "email": "jane.smith@example.com",
    "firstName": "Jane",
    "lastName": "Smith",
    "isActive": true,
    "isAdmin": true,
    "createdAt": "2024-01-14T09:00:00Z",
    "updatedAt": "2024-01-15T11:20:00Z"
  }
]
```

### Update User

**Request:**
```bash
PUT /api/users/550e8400-e29b-41d4-a716-446655440000
Content-Type: application/json

{
  "firstName": "Jonathan",
  "bio": "Senior Software Engineer",
  "location": "Austin, TX"
}
```

**Response (200 OK):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "email": "john.doe@example.com",
  "firstName": "Jonathan",
  "lastName": "Doe",
  "bio": "Senior Software Engineer",
  "location": "Austin, TX",
  "avatarUrl": "https://example.com/avatar.jpg",
  "isActive": true,
  "isAdmin": false,
  "lastLoginAt": "2024-01-15T14:30:00Z",
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-15T15:45:00Z"
}
```

### Update Last Login

**Request:**
```bash
POST /api/users/550e8400-e29b-41d4-a716-446655440000/last-login
```

**Response (204 No Content)**

### Delete User

**Request:**
```bash
DELETE /api/users/550e8400-e29b-41d4-a716-446655440000
```

**Response (204 No Content)**

## OAuth Providers API

### Create OAuth Provider

**Request:**
```bash
POST /api/oauthproviders
Content-Type: application/json

{
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "provider": 0,
  "providerId": "google-12345678",
  "providerEmail": "john.doe@gmail.com",
  "accessToken": "ya29.a0AfH6SMCX...",
  "refreshToken": "1//0gHqOzM...",
  "tokenExpiresAt": "2024-01-15T18:30:00Z"
}
```

**Provider Types:**
- `0` = Google
- `1` = GitHub
- `2` = LinkedIn
- `3` = Facebook

**Response (201 Created):**
```json
{
  "id": "770e8400-e29b-41d4-a716-446655440002",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "provider": 0,
  "providerId": "google-12345678",
  "providerEmail": "john.doe@gmail.com",
  "tokenExpiresAt": "2024-01-15T18:30:00Z",
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-15T10:30:00Z"
}
```

### Get OAuth Provider by ID

**Request:**
```bash
GET /api/oauthproviders/770e8400-e29b-41d4-a716-446655440002
```

**Response (200 OK):**
```json
{
  "id": "770e8400-e29b-41d4-a716-446655440002",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "provider": 0,
  "providerId": "google-12345678",
  "providerEmail": "john.doe@gmail.com",
  "tokenExpiresAt": "2024-01-15T18:30:00Z",
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-15T10:30:00Z"
}
```

### Get OAuth Providers for User

**Request:**
```bash
GET /api/oauthproviders/user/550e8400-e29b-41d4-a716-446655440000
```

**Response (200 OK):**
```json
[
  {
    "id": "770e8400-e29b-41d4-a716-446655440002",
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "provider": 0,
    "providerId": "google-12345678",
    "providerEmail": "john.doe@gmail.com",
    "tokenExpiresAt": "2024-01-15T18:30:00Z",
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-15T10:30:00Z"
  },
  {
    "id": "880e8400-e29b-41d4-a716-446655440003",
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "provider": 1,
    "providerId": "github-87654321",
    "providerEmail": "john.doe@users.noreply.github.com",
    "tokenExpiresAt": null,
    "createdAt": "2024-01-15T11:00:00Z",
    "updatedAt": "2024-01-15T11:00:00Z"
  }
]
```

### Update OAuth Provider

**Request:**
```bash
PUT /api/oauthproviders/770e8400-e29b-41d4-a716-446655440002
Content-Type: application/json

{
  "accessToken": "ya29.a0AfH6SMCY...",
  "refreshToken": "1//0gHqOzN...",
  "tokenExpiresAt": "2024-01-15T20:30:00Z"
}
```

**Response (200 OK):**
```json
{
  "id": "770e8400-e29b-41d4-a716-446655440002",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "provider": 0,
  "providerId": "google-12345678",
  "providerEmail": "john.doe@gmail.com",
  "tokenExpiresAt": "2024-01-15T20:30:00Z",
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-15T16:00:00Z"
}
```

## User Analytics API

### Create Analytics Event

**Request:**
```bash
POST /api/useranalytics
Content-Type: application/json

{
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "sessionId": "sess_abc123xyz",
  "eventType": "page_view",
  "eventData": "{\"page\": \"/dashboard\", \"duration\": 45}",
  "ipAddress": "192.168.1.100",
  "userAgent": "Mozilla/5.0...",
  "referrerUrl": "https://google.com"
}
```

**Response (201 Created):**
```json
{
  "id": "990e8400-e29b-41d4-a716-446655440004",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "sessionId": "sess_abc123xyz",
  "eventType": "page_view",
  "eventData": "{\"page\": \"/dashboard\", \"duration\": 45}",
  "ipAddress": "192.168.1.100",
  "userAgent": "Mozilla/5.0...",
  "referrerUrl": "https://google.com",
  "createdAt": "2024-01-15T14:30:00Z"
}
```

### Get Analytics for User

**Request:**
```bash
GET /api/useranalytics/user/550e8400-e29b-41d4-a716-446655440000
```

**Response (200 OK):**
```json
[
  {
    "id": "990e8400-e29b-41d4-a716-446655440004",
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "sessionId": "sess_abc123xyz",
    "eventType": "page_view",
    "eventData": "{\"page\": \"/dashboard\"}",
    "ipAddress": "192.168.1.100",
    "createdAt": "2024-01-15T14:30:00Z"
  },
  {
    "id": "aa0e8400-e29b-41d4-a716-446655440005",
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "sessionId": "sess_abc123xyz",
    "eventType": "button_click",
    "eventData": "{\"button\": \"save\"}",
    "ipAddress": "192.168.1.100",
    "createdAt": "2024-01-15T14:32:00Z"
  }
]
```

### Get Analytics by Date Range

**Request:**
```bash
GET /api/useranalytics/date-range?startDate=2024-01-15T00:00:00Z&endDate=2024-01-15T23:59:59Z
```

**Response (200 OK):**
```json
[
  {
    "id": "990e8400-e29b-41d4-a716-446655440004",
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "sessionId": "sess_abc123xyz",
    "eventType": "page_view",
    "eventData": "{\"page\": \"/dashboard\"}",
    "ipAddress": "192.168.1.100",
    "createdAt": "2024-01-15T14:30:00Z"
  }
]
```

## Error Responses

### 400 Bad Request

```json
{
  "errors": {
    "Email": ["The Email field is required."],
    "FirstName": ["The FirstName field must be a string with a maximum length of 100."]
  }
}
```

### 404 Not Found

```json
"User with ID 550e8400-e29b-41d4-a716-446655440000 not found"
```

### 500 Internal Server Error

```json
"An error occurred while processing your request."
```

## Health Check

**Request:**
```bash
GET /health
```

**Response (200 OK):**
```
Healthy
```

**Response (503 Service Unavailable):**
```
Unhealthy
```

