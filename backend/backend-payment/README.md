# Payment Service API

A comprehensive payment service built with ASP.NET Core 9.0 and Stripe integration, following SOLID principles and design patterns.

## Features

- **Stripe Checkout Integration** - Secure payment processing with Stripe Checkout
- **Webhook Support** - Real-time payment status updates via Stripe webhooks
- **Idempotency** - Safe retry mechanism for payment operations
- **SOLID Architecture** - Repository pattern, Unit of Work, Dependency Injection
- **Clean API** - RESTful endpoints with comprehensive documentation
- **PostgreSQL Database** - Persistent payment and idempotency key storage
- **Health Checks** - Built-in health monitoring endpoints

## Architecture

This service implements a clean, layered architecture with:

- **Model** - Domain entities (Payment, IdempotencyKey)
- **DTOs** - Request/Response data transfer objects
- **Repositories** - Data access layer with Unit of Work pattern
- **Services** - Business logic layer (PaymentService, StripeService)
- **Controllers** - API endpoints
- **Config** - Modular configuration classes

## Prerequisites

- .NET 9.0 SDK
- PostgreSQL 15+
- Stripe Account (Test mode keys)

## Getting Started

### 1. Database Setup

The payment service expects a PostgreSQL database with the following schema (located in `/db/schemas/payments_service.sql`):

```sql
-- Payments table
CREATE TABLE payments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id UUID,
    user_id UUID,
    amount NUMERIC(12,2) NOT NULL,
    currency CHAR(3) DEFAULT 'USD',
    provider VARCHAR(50) NOT NULL DEFAULT 'stripe',
    provider_payment_id VARCHAR(255),
    provider_charge_id VARCHAR(255),
    status VARCHAR(50) NOT NULL DEFAULT 'processing',
    raw_provider_response JSONB,
    failure_message TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Idempotency keys table
CREATE TABLE idempotency_keys (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    key VARCHAR(255) UNIQUE NOT NULL,
    user_id UUID,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    expires_at TIMESTAMPTZ
);
```

### 2. Environment Configuration

Copy `.env.example` to `.env.local` and configure:

```bash
# PostgreSQL
POSTGRES_DB=payments_db
POSTGRES_USER=payments_user
POSTGRES_PASSWORD=payments_pass

# Stripe API Keys (get from https://dashboard.stripe.com/test/apikeys)
Stripe__SecretKey=sk_test_your_secret_key_here
Stripe__WebhookSecret=whsec_your_webhook_secret_here
```

### 3. Update appsettings

Update `appsettings.Development.json` with your Stripe keys:

```json
{
  "Stripe": {
    "PublishableKey": "pk_test_your_publishable_key",
    "SecretKey": "sk_test_your_secret_key",
    "WebhookSecret": "whsec_your_webhook_secret"
  }
}
```

### 4. Run the Service

```bash
# Restore packages
dotnet restore

# Run migrations (if using EF migrations)
dotnet ef database update

# Run the service
dotnet run
```

The service will start on `http://localhost:5001` (or `https://localhost:7153`).

## API Endpoints

### Payments

- `GET /api/payments/{id}` - Get payment by ID
- `GET /api/payments/order/{orderId}` - Get payment by order ID
- `GET /api/payments/user/{userId}` - Get all payments for a user
- `POST /api/payments/checkout-session` - Create Stripe checkout session
- `GET /api/payments/verify/{sessionId}` - Verify payment status
- `POST /api/payments/webhook` - Stripe webhook endpoint

### Health Checks

- `GET /health` - Service health status

## Usage Example

### Creating a Checkout Session

```bash
curl -X POST http://localhost:5001/api/payments/checkout-session \
  -H "Content-Type: application/json" \
  -d '{
    "orderId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa7",
    "lineItems": [
      {
        "name": "Burger",
        "description": "Classic cheeseburger",
        "unitPrice": 8.99,
        "quantity": 2,
        "imageUrl": "https://example.com/burger.jpg"
      }
    ],
    "successUrl": "http://localhost:3000/checkout/success?session_id={CHECKOUT_SESSION_ID}",
    "cancelUrl": "http://localhost:3000/checkout",
    "idempotencyKey": "checkout_1234567890"
  }'
```

Response:
```json
{
  "sessionId": "cs_test_...",
  "sessionUrl": "https://checkout.stripe.com/c/pay/cs_test_...",
  "paymentId": "3fa85f64-5717-4562-b3fc-2c963f66afa8",
  "message": "Checkout session created successfully"
}
```

## Stripe Webhook Configuration

### Local Testing with Stripe CLI

1. Install Stripe CLI:
```bash
brew install stripe/stripe-cli/stripe
```

2. Login to Stripe:
```bash
stripe login
```

3. Forward webhooks to local server:
```bash
stripe listen --forward-to localhost:5001/api/payments/webhook
```

4. Copy the webhook signing secret and update your `.env.local`:
```
Stripe__WebhookSecret=whsec_your_webhook_secret_here
```

### Production Webhook Setup

1. Go to Stripe Dashboard > Developers > Webhooks
2. Add endpoint: `https://your-domain.com/api/payments/webhook`
3. Select events to listen to:
   - `checkout.session.completed`
   - `payment_intent.succeeded`
   - `payment_intent.payment_failed`
4. Copy the webhook signing secret to your production environment variables

## Payment Flow

1. **User initiates checkout** - Frontend calls `POST /api/payments/checkout-session`
2. **Create Stripe session** - Backend creates a Stripe Checkout session and payment record
3. **Redirect to Stripe** - Frontend redirects user to Stripe Checkout URL
4. **User completes payment** - User enters payment details on Stripe
5. **Webhook notification** - Stripe sends webhook to backend
6. **Update payment status** - Backend processes webhook and updates payment record
7. **User redirected** - Stripe redirects user back to success URL
8. **Verify payment** - Frontend calls `GET /api/payments/verify/{sessionId}` to confirm

## Design Patterns

### Repository Pattern
Abstracts data access logic, making it easier to test and maintain.

### Unit of Work Pattern
Manages transactions and coordinates the work of multiple repositories.

### Result Pattern
Provides a consistent way to handle success and failure outcomes.

### Dependency Injection
All dependencies are injected, making the system testable and flexible.

### Strategy Pattern
Different payment providers can be implemented without changing existing code.

## Testing

### Unit Tests
```bash
dotnet test
```

### Integration Tests with Stripe Test Mode

Use Stripe's test card numbers:
- Success: `4242 4242 4242 4242`
- Declined: `4000 0000 0000 0002`
- Requires authentication: `4000 0025 0000 3155`

## Security Considerations

1. **API Keys** - Never commit API keys to version control
2. **Webhook Signatures** - Always verify webhook signatures
3. **HTTPS** - Use HTTPS in production
4. **CORS** - Configure CORS to only allow trusted origins
5. **Idempotency** - Use idempotency keys to prevent duplicate charges

## Troubleshooting

### Common Issues

1. **Webhook signature verification failed**
   - Ensure webhook secret is correct
   - Check that request body is not modified before verification

2. **Database connection failed**
   - Verify PostgreSQL is running
   - Check connection string in appsettings

3. **Stripe API errors**
   - Verify API keys are correct
   - Check Stripe Dashboard for error details

## License

This project is part of the CampusEats platform.

