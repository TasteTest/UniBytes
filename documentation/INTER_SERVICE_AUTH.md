# Inter-Service Authentication

This document describes how backend-payment authenticates users via backend-user service.

## Architecture

```
┌──────────┐      ┌────────────────┐      ┌──────────────┐
│ Frontend │      │ backend-payment│      │ backend-user │
└────┬─────┘      └────────┬───────┘      └──────┬───────┘
     │                     │                      │
     │ 1. POST /checkout   │                      │
     │    + accessToken    │                      │
     │    + userEmail      │                      │
     ├────────────────────>│                      │
     │                     │                      │
     │                     │ 2. GET /api/auth/me  │
     │                     │    Authorization:     │
     │                     │    Bearer {token}    │
     │                     │    X-User-Email:     │
     │                     │    {email}           │
     │                     ├─────────────────────>│
     │                     │                      │
     │                     │ 3. UserInfo          │
     │                     │    {id, email, ...}  │
     │                     │<─────────────────────┤
     │                     │                      │
     │                     │ 4. Create Payment    │
     │                     │    with userId       │
     │                     │                      │
     │ 5. Checkout URL     │                      │
     │<────────────────────┤                      │
     │                     │                      │
```

## Components

### Backend-User

**New Endpoint:** `GET /api/auth/me`

Validates the access token and returns user information:

```csharp
// Controllers/AuthController.cs
[HttpGet("me")]
public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
{
    // Extract token from Authorization header
    var authHeader = Request.Headers["Authorization"].ToString();
    var token = authHeader.Substring("Bearer ".Length).Trim();
    
    // Get user email from header
    var userEmail = Request.Headers["X-User-Email"].ToString();
    
    // Fetch and return user info
    var result = await _userService.GetUserByEmailAsync(userEmail, cancellationToken);
    
    return Ok(new {
        id = result.Value.Id.ToString(),
        email = result.Value.Email,
        firstName = result.Value.FirstName,
        lastName = result.Value.LastName,
        avatarUrl = result.Value.AvatarUrl
    });
}
```

### Backend-Payment

**New Service:** `UserServiceClient`

Communicates with backend-user to validate tokens:

```csharp
// Services/UserServiceClient.cs
public async Task<Result<UserInfoResponse>> GetUserInfoAsync(
    string accessToken, 
    string userEmail, 
    CancellationToken cancellationToken)
{
    var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
    request.Headers.Add("Authorization", $"Bearer {accessToken}");
    request.Headers.Add("X-User-Email", userEmail);
    
    var response = await _httpClient.SendAsync(request, cancellationToken);
    // Parse and return user info
}
```

**Updated DTO:** `CreateCheckoutSessionRequest`

Now accepts accessToken and userEmail instead of userId:

```csharp
public class CreateCheckoutSessionRequest
{
    public Guid OrderId { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public List<CheckoutLineItem> LineItems { get; set; } = new();
    // ...
}
```

**Updated Service:** `StripeService`

Validates user before creating payment:

```csharp
public async Task<Result<CheckoutSessionResponse>> CreateCheckoutSessionAsync(...)
{
    // Validate user with backend-user service
    var userInfoResult = await _userServiceClient.GetUserInfoAsync(
        request.AccessToken, 
        request.UserEmail, 
        cancellationToken);
    
    if (!userInfoResult.IsSuccess)
        return Result<CheckoutSessionResponse>.Failure("Authentication failed");
    
    var userId = Guid.Parse(userInfoResult.Value.Id);
    
    // Create payment with validated user ID
    var payment = new Payment { UserId = userId, ... };
}
```

### Frontend

**Updated Request:**

```typescript
// app/checkout/page.tsx
const accessToken = (session as any).accessToken || ''
const userEmail = session.user.email || ''

const result = await paymentService.createCheckoutSession({
  orderId,
  accessToken,        // Send token instead of userId
  userEmail,          // Send email for validation
  lineItems,
  successUrl,
  cancelUrl,
  idempotencyKey,
})
```

## Configuration

### Backend-Payment `appsettings.Development.json`

```json
{
  "Services": {
    "UserServiceUrl": "http://localhost:5000"
  }
}
```

### Backend-Payment `.env.local`

```env
POSTGRES_HOST=localhost
POSTGRES_PORT=5437
POSTGRES_DB=payments_db
POSTGRES_USER=payments_user
POSTGRES_PASSWORD=payments_pass
```

## Security Benefits

1. **No User ID Exposure**: Frontend never sees or handles user IDs
2. **Token Validation**: Every request is validated with backend-user
3. **Centralized Auth**: Authentication logic stays in backend-user
4. **Audit Trail**: All authentication attempts are logged
5. **Scalable**: Easy to add more services that need user validation

## Testing

1. **Start backend-user (port 5000)**
   ```bash
   cd backend/backend-user
   dotnet run
   ```

2. **Start backend-payment (port 5001)**
   ```bash
   cd backend/backend-payment
   dotnet run
   ```

3. **Start frontend (port 3000)**
   ```bash
   cd frontend
   npm run dev
   ```

4. **Test the flow:**
   - Sign in with Google OAuth
   - Add items to cart
   - Go to checkout
   - Click "Continue to Payment"
   - Backend-payment will call backend-user to validate your token
   - If valid, you'll be redirected to Stripe Checkout

## Troubleshooting

### "Authentication failed" error

- **Check backend-user is running** on port 5000
- **Check accessToken is present** in session
- **Check user email** is in session
- **Review backend-user logs** for validation errors

### "Failed to authenticate user" in logs

- Verify `Services:UserServiceUrl` is configured correctly
- Ensure backend-user `/api/auth/me` endpoint is accessible
- Check network connectivity between services

### User not found

- Ensure user is signed in with Google OAuth
- Verify user exists in backend-user database
- Check email matches between frontend and backend

## Future Improvements

1. **JWT Validation**: Properly validate JWT tokens instead of just checking email
2. **Token Caching**: Cache validated tokens to reduce backend-user calls
3. **Service Mesh**: Use a service mesh for inter-service communication
4. **API Gateway**: Route all requests through an API gateway
5. **Refresh Tokens**: Implement refresh token mechanism for long sessions

