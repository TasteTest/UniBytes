# Loyalty Service - Frontend Integration Guide

## Overview
This document describes the integration between the backend-loyalty microservice and the Next.js frontend.

## Architecture

### Backend (backend-loyalty)
- **Framework**: .NET 9.0 / ASP.NET Core
- **Database**: PostgreSQL (loyalty_db)
- **Port**: 5024
- **API Base**: http://localhost:5024/api

### Frontend
- **Framework**: Next.js with TypeScript
- **API Client**: Custom ApiClient class with Result pattern
- **State Management**: React hooks (useState, useEffect)

## File Structure

### Frontend Files Created

```
frontend/
├── lib/
│   ├── types/
│   │   └── loyalty.types.ts          # TypeScript type definitions
│   ├── api/
│   │   ├── loyalty.ts                # Loyalty service API client
│   │   └── endpoints.ts              # Updated with loyalty endpoints
│   └── config/
│       └── rewards.ts                # Rewards catalog configuration
├── app/
│   └── loyalty/
│       └── page.tsx                  # Updated loyalty page (real API calls)
└── .env.local                        # Environment configuration
```

### Type Definitions (`lib/types/loyalty.types.ts`)

Matches backend DTOs:
- `LoyaltyAccount` - Account data with points, tier, status
- `LoyaltyAccountDetails` - Full details with transaction/redemption history
- `LoyaltyTransaction` - Points earned tracking
- `LoyaltyRedemption` - Reward redemption records
- `AddPointsRequest` / `RedeemPointsRequest` - Action requests
- `Reward` - Frontend reward catalog type

### API Client (`lib/api/loyalty.ts`)

**LoyaltyService** class provides:
- `getByUserId(userId)` - Get account by user ID
- `getAccountDetails(userId)` - Get full details with history
- `getBalance(userId)` - Get points balance only
- `getOrCreateAccount(userId)` - Get existing or create new account
- `addPoints(request)` - Add points to account
- `redeemPoints(request)` - Redeem points for reward
- `getTierName(tier)` / `getTierColor(tier)` - UI helpers

### Rewards Configuration (`lib/config/rewards.ts`)

Defines:
- `AVAILABLE_REWARDS` - Catalog of redeemable rewards
- `POINTS_EARNING_RATE` - Points per dollar and tier multipliers
- `TIER_THRESHOLDS` - Points needed for tier advancement
- Helper functions for points calculation

## API Endpoints

All endpoints are prefixed with `/api/loyaltyaccounts`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/user/{userId}` | Get account by user ID |
| GET | `/user/{userId}/details` | Get full details with history |
| GET | `/user/{userId}/balance` | Get points balance |
| GET | `/` | Get all accounts (admin) |
| GET | `/active` | Get all active accounts |
| GET | `/tier/{tier}` | Get accounts by tier |
| POST | `/` | Create new account |
| POST | `/add-points` | Add points to account |
| POST | `/redeem-points` | Redeem points for reward |
| PUT | `/{id}` | Update account |
| DELETE | `/{id}` | Delete account |

## Data Flow

### Loading Loyalty Data
1. Component mounts → `loadLoyaltyData()` called
2. Call `loyaltyService.getOrCreateAccount(userId)`
   - If account exists, returns it
   - If not exists, creates new account with 0 points
3. Call `loyaltyService.getAccountDetails(userId)`
   - Returns account + recent transactions + recent redemptions
4. Update component state with data

### Redeeming Rewards
1. User clicks "Redeem" button on reward
2. Component calls `loyaltyService.redeemPoints()`
   - Sends: userId, points, rewardType, rewardMetadata
3. Backend validates:
   - Sufficient points
   - Account active
4. Backend deducts points and creates redemption record
5. Frontend reloads data to show updated balance
6. Shows success toast notification

### Error Handling
- API calls return `Result<T>` type: `{ isSuccess, data, error }`
- Check `isSuccess` before accessing `data`
- Display error messages via toast notifications
- Loading states prevent double-clicks during async operations

## Environment Configuration

### `.env.local`
```env
NEXT_PUBLIC_LOYALTY_API_URL=http://localhost:5024/api
```

This variable is used by the `LoyaltyService` constructor to set the base URL.

## Running the Application

### 1. Start Backend Services

**Database (PostgreSQL)**:
```powershell
docker-compose up -d loyalty_db
```

**Loyalty Service**:
```powershell
cd backend/backend-loyalty
dotnet run
```

Service will start on http://localhost:5024

### 2. Start Frontend

```powershell
cd frontend
npm run dev
```

Frontend will start on http://localhost:3000

### 3. Access Loyalty Page

Navigate to: http://localhost:3000/loyalty

## Current Limitations & TODOs

### User Authentication
Currently uses hardcoded demo user ID:
```typescript
const userId = "3fa85f64-5717-4562-b3fc-2c963f66afa6"
```

**TODO**: Replace with actual user ID from authentication context/session.

### Reward Images
Rewards don't have images yet.

**TODO**: Add image URLs to `AVAILABLE_REWARDS` and display them in the UI.

### Tier Advancement
Tiers are currently assigned manually via the API.

**TODO**: Implement automatic tier advancement based on `totalPointsEarned` thresholds.

### Real-time Updates
Points balance updates only on page load/refresh.

**TODO**: Consider WebSocket integration for real-time points updates.

### Pagination
Transaction/redemption history shows only recent 5 items.

**TODO**: Add pagination or "load more" functionality.

## Testing

### Using Swagger UI
1. Navigate to http://localhost:5024/swagger
2. Test endpoints directly with JSON payloads

### Using the Frontend
1. Visit http://localhost:3000/loyalty
2. View current points balance
3. Browse available rewards
4. Click "Redeem" on rewards you can afford
5. Check "Activity Log" and "Redemption History"

### Testing Points Addition
Since points are earned through orders, you'll need to:
1. Use Swagger to manually add points: POST `/add-points`
2. Or integrate with the orders service to automatically award points

Example add points request:
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "points": 100,
  "reason": "Order #ORD-123",
  "referenceId": "order-uuid-here"
}
```

## Integration with Other Services

### Orders Service Integration
To automatically award points on order completion:

1. When order is completed, call loyalty service:
```typescript
await loyaltyService.addPoints({
  userId: order.userId,
  points: calculatePointsEarned(order.total),
  reason: `Order #${order.orderNumber}`,
  referenceId: order.id
})
```

2. Calculate points based on order total:
```typescript
const pointsPerDollar = 10
const points = Math.floor(order.total * pointsPerDollar)
```

### Payment Service Integration
Could offer bonus points for certain payment methods or during promotions.

## Database Schema

### Tables
- `loyalty_accounts` - User loyalty accounts
- `loyalty_transactions` - Points earned/spent history
- `loyalty_redemptions` - Reward redemption records

### Key Fields
- `points_balance` - Current available points
- `tier` - 0=Bronze, 1=Silver, 2=Gold, 3=Platinum
- `metadata` - JSONB field for flexible data storage

## API Response Examples

### Get Account Details
```json
{
  "account": {
    "id": "uuid",
    "userId": "uuid",
    "pointsBalance": 175,
    "tier": 0,
    "tierName": "Bronze",
    "isActive": true,
    "createdAt": "2024-11-26T10:00:00Z",
    "updatedAt": "2024-11-26T12:00:00Z"
  },
  "recentTransactions": [
    {
      "id": "uuid",
      "loyaltyAccountId": "uuid",
      "changeAmount": 150,
      "reason": "Order #ORD-001",
      "referenceId": "order-uuid",
      "metadata": "{}",
      "createdAt": "2024-11-26T12:00:00Z"
    }
  ],
  "recentRedemptions": [],
  "totalPointsEarned": 500,
  "totalPointsRedeemed": 325
}
```

### Redeem Points Response
```json
{
  "id": "uuid",
  "loyaltyAccountId": "uuid",
  "pointsUsed": 100,
  "rewardType": "MenuItem",
  "rewardMetadata": "{\"rewardId\":\"reward-free-drink\",\"rewardName\":\"Free Drink\"}",
  "createdAt": "2024-11-26T13:00:00Z"
}
```

## Support

For issues or questions:
1. Check Swagger documentation at http://localhost:5024/swagger
2. Review backend logs in the terminal
3. Check browser console for frontend errors
4. Verify database connection in PostgreSQL
