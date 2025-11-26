# Backend Loyalty Service - Implementation Complete

## ✅ What Has Been Implemented

### 1. **Data Layer** (`Data/`)
- `ApplicationDbContext.cs` - EF Core DbContext with loyalty tables
- **Configurations/**
  - `LoyaltyAccountConfiguration.cs` - Account table configuration
  - `LoyaltyTransactionConfiguration.cs` - Transaction history configuration
  - `LoyaltyRedemptionConfiguration.cs` - Redemption records configuration

### 2. **Model Layer** (`Model/`)
- `LoyaltyAccount.cs` - Main account entity with user reference, points, tier
- `LoyaltyTransaction.cs` - Points change tracking
- `LoyaltyRedemption.cs` - Redemption history

### 3. **Common Layer** (`Common/`)
- `BaseEntity.cs` - Base class with Id, CreatedAt, UpdatedAt
- `Result.cs` - Result pattern for operation outcomes
- **Enums/**
  - `LoyaltyTier.cs` - Bronze, Silver, Gold, Platinum tiers

### 4. **Repository Layer** (`Repositories/`)
- `Repository.cs` - Generic repository implementation
- `LoyaltyAccountRepository.cs` - Account-specific operations
- `LoyaltyTransactionRepository.cs` - Transaction queries
- `LoyaltyRedemptionRepository.cs` - Redemption queries
- `UnitOfWork.cs` - Transaction management
- **Interfaces/** for all repositories

### 5. **Service Layer** (`Services/`)
- `LoyaltyAccountService.cs` - Business logic for:
  - Creating/managing accounts
  - Adding points
  - Redeeming points
  - Checking balances
  - Transaction history

### 6. **DTO Layer** (`DTOs/`)
- **Request DTOs:**
  - `CreateLoyaltyAccountRequest.cs`
  - `UpdateLoyaltyAccountRequest.cs`
  - `AddPointsRequest.cs`
  - `RedeemPointsRequest.cs`
- **Response DTOs:**
  - `LoyaltyAccountResponse.cs`
  - `LoyaltyTransactionResponse.cs`
  - `LoyaltyRedemptionResponse.cs`
  - `LoyaltyAccountDetailsResponse.cs`

### 7. **Controller Layer** (`Controllers/`)
- `LoyaltyAccountsController.cs` - REST API endpoints for:
  - GET `/api/loyaltyaccounts` - Get all accounts
  - GET `/api/loyaltyaccounts/user/{userId}` - Get by user ID
  - GET `/api/loyaltyaccounts/user/{userId}/details` - Get account with history
  - GET `/api/loyaltyaccounts/user/{userId}/balance` - Check points balance
  - POST `/api/loyaltyaccounts` - Create account
  - POST `/api/loyaltyaccounts/add-points` - Add points
  - POST `/api/loyaltyaccounts/redeem-points` - Redeem points

### 8. **Configuration** (`Config/`)
- `DatabaseConfiguration.cs` - PostgreSQL setup
- `RepositoryConfiguration.cs` - DI for repositories
- `ServiceConfiguration.cs` - DI for services
- `AutoMapperConfiguration.cs` - AutoMapper setup
- `CorsConfiguration.cs` - CORS policy
- `SwaggerConfiguration.cs` - API documentation
- `HealthCheckConfiguration.cs` - Health endpoints

### 9. **Mappings**
- `MappingProfile.cs` - AutoMapper mappings between models and DTOs

---

## 🔧 What You Need To Do Next

### Step 1: Update the .csproj file

Add the required NuGet packages to `backend-loyalty.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

    <PropertyGroup>
        <TargetFramework>net9.0</TargetFramework>
        <Nullable>enable</Nullable>
        <ImplicitUsings>enable</ImplicitUsings>
        <RootNamespace>backend_loyalty</RootNamespace>
        <GenerateDocumentationFile>true</GenerateDocumentationFile>
        <NoWarn>$(NoWarn);1591</NoWarn>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="9.0.7"/>
        <PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.0"/>
        <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.0">
            <PrivateAssets>all</PrivateAssets>
            <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
        </PackageReference>
        <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.0"/>
        <PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1"/>
        <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0"/>
        <PackageReference Include="AspNetCore.HealthChecks.NpgSql" Version="8.0.1"/>
    </ItemGroup>

</Project>
```

### Step 2: Restore NuGet Packages

```powershell
cd backend\backend-loyalty
dotnet restore
```

### Step 3: Create Database Migrations

```powershell
# Create initial migration
dotnet ef migrations add InitialCreate

# Apply migration to database
dotnet ef database update
```

### Step 4: Update appsettings.json

Add database connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5433;Database=loyalty_db;Username=loyalty_user;Password=loyalty_pass"
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:5173"
    ]
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Step 5: Run and Test

```powershell
dotnet run
```

The API will be available at:
- Swagger UI: `https://localhost:7XXX/swagger`
- Health Check: `https://localhost:7XXX/health`

---

## 🎯 How to Implement Points-for-Menu-Items Feature

To allow users to redeem points for menu items without payment, you need to integrate the loyalty service with the orders and payments services:

### Architecture Overview

```
User places order → Check payment method → If "Points":
  1. Check loyalty balance
  2. Calculate required points
  3. Validate sufficient balance
  4. Deduct points
  5. Create order without payment
  6. Record redemption
```

### Implementation Steps

#### 1. **Add Points Calculation to Menu Service**

In `backend-menu`, add a field to menu items for points value:

```csharp
// In MenuItem model
public long? PointsCost { get; set; } // Points required to redeem this item
```

This allows you to set how many points each menu item costs.

#### 2. **Create Payment Method Enum in Orders Service**

In `backend-order`, add payment method tracking:

```csharp
public enum PaymentMethod
{
    Card = 0,
    LoyaltyPoints = 1,
    Mixed = 2  // Partial points + partial payment
}

// In Order model
public PaymentMethod PaymentMethod { get; set; }
public long? PointsUsed { get; set; }
```

#### 3. **Add Loyalty Integration to Orders Service**

Create a loyalty service client in `backend-order`:

```csharp
public interface ILoyaltyServiceClient
{
    Task<long> GetPointsBalanceAsync(Guid userId);
    Task<bool> RedeemPointsAsync(Guid userId, long points, Guid orderId);
    Task<bool> RefundPointsAsync(Guid userId, long points, Guid orderId);
}

public class LoyaltyServiceClient : ILoyaltyServiceClient
{
    private readonly HttpClient _httpClient;
    
    public LoyaltyServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:5XXX"); // Loyalty service URL
    }
    
    public async Task<long> GetPointsBalanceAsync(Guid userId)
    {
        var response = await _httpClient.GetAsync($"/api/loyaltyaccounts/user/{userId}/balance");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<long>();
    }
    
    public async Task<bool> RedeemPointsAsync(Guid userId, long points, Guid orderId)
    {
        var request = new
        {
            UserId = userId,
            Points = points,
            RewardType = "MenuItemPurchase",
            RewardMetadata = $"{{\"orderId\":\"{orderId}\"}}"
        };
        
        var response = await _httpClient.PostAsJsonAsync("/api/loyaltyaccounts/redeem-points", request);
        return response.IsSuccessStatusCode;
    }
    
    public async Task<bool> RefundPointsAsync(Guid userId, long points, Guid orderId)
    {
        var request = new
        {
            UserId = userId,
            Points = points,
            Reason = $"Order {orderId} cancelled - refund",
            ReferenceId = orderId
        };
        
        var response = await _httpClient.PostAsJsonAsync("/api/loyaltyaccounts/add-points", request);
        return response.IsSuccessStatusCode;
    }
}
```

#### 4. **Modify Order Creation Logic**

In your order service, update the order creation:

```csharp
public async Task<Result<OrderResponse>> CreateOrderAsync(CreateOrderRequest request)
{
    // Calculate total
    var totalAmount = CalculateTotal(request.Items);
    var requiredPoints = CalculatePointsCost(request.Items);
    
    if (request.PaymentMethod == PaymentMethod.LoyaltyPoints)
    {
        // Check if user has enough points
        var balance = await _loyaltyClient.GetPointsBalanceAsync(request.UserId);
        
        if (balance < requiredPoints)
        {
            return Result<OrderResponse>.Failure(
                $"Insufficient points. Required: {requiredPoints}, Available: {balance}");
        }
        
        // Create order with pending status
        var order = CreateOrder(request);
        await _unitOfWork.Orders.AddAsync(order);
        await _unitOfWork.SaveChangesAsync();
        
        // Redeem points
        var redeemed = await _loyaltyClient.RedeemPointsAsync(
            request.UserId, 
            requiredPoints, 
            order.Id);
        
        if (!redeemed)
        {
            // Rollback order creation
            await _unitOfWork.Orders.DeleteAsync(order);
            await _unitOfWork.SaveChangesAsync();
            return Result<OrderResponse>.Failure("Failed to redeem points");
        }
        
        // Mark order as paid via points
        order.OrderStatus = OrderStatus.Confirmed;
        order.PointsUsed = requiredPoints;
        await _unitOfWork.SaveChangesAsync();
        
        return Result<OrderResponse>.Success(_mapper.Map<OrderResponse>(order));
    }
    
    // Normal payment flow...
}
```

#### 5. **Add Points Earning on Completed Orders**

When an order is completed (paid with money), award points:

```csharp
public async Task<Result> CompleteOrderAsync(Guid orderId)
{
    var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
    
    if (order.PaymentMethod != PaymentMethod.LoyaltyPoints)
    {
        // Award points (e.g., 1 point per dollar spent)
        var pointsToAward = (long)(order.TotalAmount * 10); // 10 points per dollar
        
        await _loyaltyClient.AddPointsAsync(new AddPointsRequest
        {
            UserId = order.UserId,
            Points = pointsToAward,
            Reason = "Order purchase",
            ReferenceId = orderId
        });
    }
    
    // Update order status
    order.OrderStatus = OrderStatus.Completed;
    await _unitOfWork.SaveChangesAsync();
    
    return Result.Success();
}
```

#### 6. **Update Frontend to Show Points Option**

In the frontend checkout:

```typescript
// Fetch user's points balance
const balance = await fetch(`/api/loyaltyaccounts/user/${userId}/balance`);

// Show option to pay with points if sufficient
if (balance >= requiredPoints) {
  // Display "Pay with Points" button
  // Show conversion: "This order costs 500 points"
}

// On submit
if (paymentMethod === 'points') {
  const order = await createOrder({
    ...orderData,
    paymentMethod: 'LoyaltyPoints'
  });
}
```

### Reward Tiers Implementation

You can implement tier benefits:

```csharp
public class TierBenefits
{
    public static decimal GetDiscountMultiplier(LoyaltyTier tier)
    {
        return tier switch
        {
            LoyaltyTier.Bronze => 1.0m,   // No discount
            LoyaltyTier.Silver => 0.95m,  // 5% off
            LoyaltyTier.Gold => 0.90m,    // 10% off
            LoyaltyTier.Platinum => 0.85m // 15% off
        };
    }
    
    public static long GetPointsMultiplier(LoyaltyTier tier)
    {
        return tier switch
        {
            LoyaltyTier.Bronze => 1,   // 1x points
            LoyaltyTier.Silver => 2,   // 2x points
            LoyaltyTier.Gold => 3,     // 3x points
            LoyaltyTier.Platinum => 4  // 4x points
        };
    }
}
```

---

## 🔄 Complete Integration Flow

1. **User browses menu** → Sees both price and points cost
2. **User adds to cart** → Frontend calculates total points needed
3. **Checkout** → User chooses "Pay with Points" or "Pay with Card"
4. **If Points:**
   - Orders service calls Loyalty service to check balance
   - If sufficient, creates order
   - Loyalty service deducts points and creates redemption record
   - Order is confirmed immediately (no payment service needed)
5. **If Card:**
   - Normal payment flow
   - After successful payment, Orders service calls Loyalty service to award points
   - Points are added to user's account with order reference

---

## 📊 Database Schema

Your loyalty database will have:

```
loyalty_accounts
- id (PK)
- user_id (UNIQUE)
- points_balance
- tier (0=Bronze, 1=Silver, 2=Gold, 3=Platinum)
- is_active
- created_at
- updated_at

loyalty_transactions
- id (PK)
- loyalty_account_id (FK)
- change_amount (+/- points)
- reason
- reference_id (order_id, etc.)
- metadata (JSON)
- created_at

loyalty_redemptions
- id (PK)
- loyalty_account_id (FK)
- points_used
- reward_type ("MenuItemPurchase", "Discount", etc.)
- reward_metadata (JSON with order details)
- created_at
```

---

## 🎉 Summary

**Implemented:** Complete backend-loyalty service with all layers following clean architecture.

**Next Steps:**
1. Update .csproj with dependencies
2. Run migrations to create database
3. Test the loyalty service endpoints
4. Integrate with orders service for points redemption
5. Update frontend to show points balance and redemption options

The loyalty system is now ready to track points, handle redemptions, and integrate with your order flow!
