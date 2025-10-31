# CampusEats API Documentation

This document describes the expected API endpoints and data structures for the CampusEats frontend.

## Base URL
```
Development: http://localhost:8000/api
Production: https://api.campuseats.com
```

## Authentication

All authenticated requests should include the session token in the Authorization header:
```
Authorization: Bearer <token>
```

---

## Endpoints

### Authentication

#### Sign In
```http
POST /auth/signin
Content-Type: application/json

{
  "provider": "google",
  "token": "string"
}

Response 200:
{
  "user": {
    "id": "string",
    "email": "string",
    "name": "string",
    "image": "string"
  },
  "token": "string"
}
```

---

### Menu

#### Get All Menu Items
```http
GET /menu/items?category=<category>&available=<boolean>&search=<query>

Response 200:
{
  "items": [
    {
      "id": "string",
      "name": "string",
      "description": "string",
      "price": 0.00,
      "category": "string",
      "image": "string (optional)",
      "available": true,
      "preparationTime": 0,
      "nutrition": {
        "calories": 0,
        "protein": 0,
        "carbs": 0,
        "fat": 0
      }
    }
  ],
  "total": 0,
  "page": 1,
  "pageSize": 20
}
```

#### Get Menu Item by ID
```http
GET /menu/items/:id

Response 200:
{
  "id": "string",
  "name": "string",
  "description": "string",
  "price": 0.00,
  "category": "string",
  "image": "string",
  "available": true,
  "preparationTime": 0,
  "modifiers": [
    {
      "id": "string",
      "name": "string",
      "price": 0.00,
      "type": "checkbox|radio"
    }
  ]
}
```

#### Get Categories
```http
GET /menu/categories

Response 200:
{
  "categories": [
    {
      "id": "string",
      "name": "string",
      "itemCount": 0
    }
  ]
}
```

#### Create Menu Item (Admin)
```http
POST /menu/items
Authorization: Bearer <admin-token>
Content-Type: application/json

{
  "name": "string",
  "description": "string",
  "price": 0.00,
  "category": "string",
  "image": "string",
  "available": true,
  "preparationTime": 0
}

Response 201:
{
  "id": "string",
  "name": "string",
  ...
}
```

#### Update Menu Item (Admin)
```http
PUT /menu/items/:id
Authorization: Bearer <admin-token>
Content-Type: application/json

{
  "name": "string (optional)",
  "description": "string (optional)",
  "price": 0.00 (optional),
  ...
}

Response 200:
{
  "id": "string",
  "name": "string",
  ...
}
```

#### Delete Menu Item (Admin)
```http
DELETE /menu/items/:id
Authorization: Bearer <admin-token>

Response 204: No Content
```

---

### Orders

#### Create Order
```http
POST /orders
Authorization: Bearer <token>
Content-Type: application/json

{
  "items": [
    {
      "menuItemId": "string",
      "quantity": 0,
      "modifiers": ["string"],
      "specialInstructions": "string (optional)"
    }
  ],
  "pickupLocation": "string",
  "pickupTime": "ISO 8601 | 'asap'",
  "paymentMethod": "card|campus-card"
}

Response 201:
{
  "id": "string",
  "orderNumber": "string",
  "userId": "string",
  "items": [...],
  "status": "pending",
  "total": 0.00,
  "subtotal": 0.00,
  "tax": 0.00,
  "pickupLocation": "string",
  "pickupTime": "ISO 8601",
  "estimatedReadyTime": "ISO 8601",
  "createdAt": "ISO 8601"
}
```

#### Get User Orders
```http
GET /orders?status=<status>&limit=<number>&offset=<number>
Authorization: Bearer <token>

Response 200:
{
  "orders": [
    {
      "id": "string",
      "orderNumber": "string",
      "items": [...],
      "status": "pending|preparing|ready|completed|cancelled",
      "total": 0.00,
      "pickupLocation": "string",
      "pickupTime": "ISO 8601",
      "createdAt": "ISO 8601",
      "updatedAt": "ISO 8601"
    }
  ],
  "total": 0
}
```

#### Get Order by ID
```http
GET /orders/:id
Authorization: Bearer <token>

Response 200:
{
  "id": "string",
  "orderNumber": "string",
  "items": [
    {
      "menuItem": {...},
      "quantity": 0,
      "modifiers": [...],
      "specialInstructions": "string"
    }
  ],
  "status": "string",
  "statusHistory": [
    {
      "status": "string",
      "timestamp": "ISO 8601"
    }
  ],
  "total": 0.00,
  "subtotal": 0.00,
  "tax": 0.00,
  "pickupLocation": "string",
  "pickupTime": "ISO 8601",
  "createdAt": "ISO 8601"
}
```

#### Update Order Status (Kitchen/Admin)
```http
PATCH /orders/:id/status
Authorization: Bearer <kitchen-token>
Content-Type: application/json

{
  "status": "preparing|ready|completed|cancelled"
}

Response 200:
{
  "id": "string",
  "status": "string",
  "updatedAt": "ISO 8601"
}
```

---

### Loyalty

#### Get User Points
```http
GET /loyalty/points
Authorization: Bearer <token>

Response 200:
{
  "userId": "string",
  "points": 0,
  "tier": "bronze|silver|gold",
  "pointsToNextTier": 0
}
```

#### Get Available Rewards
```http
GET /loyalty/rewards

Response 200:
{
  "rewards": [
    {
      "id": "string",
      "name": "string",
      "description": "string",
      "pointsRequired": 0,
      "available": true,
      "expiresAt": "ISO 8601 (optional)"
    }
  ]
}
```

#### Redeem Reward
```http
POST /loyalty/redeem
Authorization: Bearer <token>
Content-Type: application/json

{
  "rewardId": "string"
}

Response 200:
{
  "success": true,
  "pointsDeducted": 0,
  "remainingPoints": 0,
  "couponCode": "string (optional)"
}
```

#### Get Points History
```http
GET /loyalty/history?limit=<number>&offset=<number>
Authorization: Bearer <token>

Response 200:
{
  "history": [
    {
      "id": "string",
      "type": "earned|redeemed|expired",
      "points": 0,
      "description": "string",
      "orderId": "string (optional)",
      "rewardId": "string (optional)",
      "createdAt": "ISO 8601"
    }
  ],
  "total": 0
}
```

---

### Users

#### Get User Profile
```http
GET /users/profile
Authorization: Bearer <token>

Response 200:
{
  "id": "string",
  "email": "string",
  "name": "string",
  "image": "string",
  "phoneNumber": "string (optional)",
  "preferences": {
    "notifications": {
      "orderUpdates": true,
      "promotions": false,
      "rewards": true
    },
    "favoriteLocations": ["string"],
    "dietaryRestrictions": ["string"]
  },
  "createdAt": "ISO 8601"
}
```

#### Update User Profile
```http
PATCH /users/profile
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "string (optional)",
  "phoneNumber": "string (optional)",
  "preferences": {
    "notifications": {...},
    "favoriteLocations": [...],
    "dietaryRestrictions": [...]
  }
}

Response 200:
{
  "id": "string",
  "email": "string",
  ...
}
```

#### Get Payment Methods
```http
GET /users/payment-methods
Authorization: Bearer <token>

Response 200:
{
  "methods": [
    {
      "id": "string",
      "type": "card|campus-card",
      "last4": "string",
      "brand": "visa|mastercard|amex",
      "expiryMonth": 0,
      "expiryYear": 0,
      "isDefault": true
    }
  ]
}
```

#### Add Payment Method
```http
POST /users/payment-methods
Authorization: Bearer <token>
Content-Type: application/json

{
  "type": "card|campus-card",
  "token": "string (from payment processor)"
}

Response 201:
{
  "id": "string",
  "type": "string",
  ...
}
```

---

### Kitchen

#### Get Active Orders (Kitchen)
```http
GET /kitchen/orders?station=<station>&status=<status>
Authorization: Bearer <kitchen-token>

Response 200:
{
  "orders": [
    {
      "id": "string",
      "orderNumber": "string",
      "items": [
        {
          "name": "string",
          "quantity": 0,
          "modifiers": ["string"],
          "station": "grill|salad|pizza|drinks"
        }
      ],
      "status": "pending|preparing|ready",
      "placedAt": "ISO 8601",
      "estimatedTime": 0
    }
  ]
}
```

---

## WebSocket Events (Future)

### Order Status Updates
```javascript
// Client subscribes
socket.emit('subscribe:order', { orderId: 'string' })

// Server broadcasts status changes
socket.on('order:status', {
  orderId: 'string',
  status: 'preparing|ready|completed',
  timestamp: 'ISO 8601'
})
```

---

## Error Responses

All errors follow this format:

```json
{
  "error": {
    "code": "ERROR_CODE",
    "message": "Human-readable error message",
    "details": {} // Optional additional details
  }
}
```

### Common Error Codes
- `400` - Bad Request (validation error)
- `401` - Unauthorized (missing/invalid token)
- `403` - Forbidden (insufficient permissions)
- `404` - Not Found
- `409` - Conflict (e.g., item already exists)
- `422` - Unprocessable Entity (business logic error)
- `500` - Internal Server Error

---

## Rate Limiting

- **Anonymous**: 60 requests/minute
- **Authenticated**: 300 requests/minute
- **Admin**: 1000 requests/minute

Rate limit headers:
```
X-RateLimit-Limit: 300
X-RateLimit-Remaining: 299
X-RateLimit-Reset: 1640000000
```

---

## Pagination

Endpoints supporting pagination use the following query parameters:
- `limit`: Number of items per page (default: 20, max: 100)
- `offset`: Number of items to skip (default: 0)

Response includes pagination metadata:
```json
{
  "data": [...],
  "pagination": {
    "total": 100,
    "limit": 20,
    "offset": 0,
    "hasMore": true
  }
}
```

---

## Date/Time Format

All timestamps use ISO 8601 format:
```
2024-01-15T14:30:00Z
```

---

## Schema References

See the database schemas in `/db/schemas/` for detailed field specifications:
- `users_service.sql`
- `menu_service.sql`
- `orders_service.sql`
- `loyalty_service.sql`
- `payments_service.sql`

---

For questions or issues, please contact the backend team or open an issue on GitHub.

