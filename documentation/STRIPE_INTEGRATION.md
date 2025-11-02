# Stripe Payment Integration Guide

Complete guide for integrating Stripe payments into the CampusEats platform.

## Overview

The payment integration uses Stripe Checkout for secure payment processing:

- **Backend**: ASP.NET Core payment service (`backend-payment`)
- **Frontend**: Next.js checkout page with Stripe redirect
- **Database**: PostgreSQL for payment records

## Quick Start

### 1. Get Stripe API Keys

1. Sign up at [https://dashboard.stripe.com/register](https://dashboard.stripe.com/register)
2. Go to Developers > API keys
3. Copy your **Publishable key** (starts with `pk_test_`)
4. Copy your **Secret key** (starts with `sk_test_`)

### 2. Configure Backend

Edit `/backend/backend-payment/.env.local`:

```env
POSTGRES_DB=payments_db
POSTGRES_USER=payments_user
POSTGRES_PASSWORD=payments_pass

# Your Stripe keys
Stripe__SecretKey=sk_test_51...
Stripe__WebhookSecret=whsec_...
```

Edit `/backend/backend-payment/appsettings.Development.json`:

```json
{
  "Stripe": {
    "PublishableKey": "pk_test_51...",
    "SecretKey": "sk_test_51...",
    "WebhookSecret": "whsec_..."
  }
}
```

### 3. Configure Frontend

Edit `/frontend/.env.local`:

```env
# Your Stripe publishable key
NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY=pk_test_51...
NEXT_PUBLIC_PAYMENT_API_URL=http://localhost:5001/api
```

### 4. Start Services

```bash
# Terminal 1: Start backend-user (port 5000)
cd backend/backend-user
dotnet run

# Terminal 2: Start backend-payment (port 5001)
cd backend/backend-payment
dotnet run

# Terminal 3: Start frontend (port 3000)
cd frontend
npm run dev
```

### 5. Setup Stripe Webhook (for local testing)

```bash
# Install Stripe CLI
brew install stripe/stripe-cli/stripe

# Login
stripe login

# Forward webhooks to local backend
stripe listen --forward-to localhost:5001/api/payments/webhook
```

Copy the webhook signing secret (starts with `whsec_`) and update it in your backend `.env.local` and `appsettings.Development.json`.

## Payment Flow

### User Journey

1. User adds items to cart
2. User clicks "Checkout"
3. User fills in pickup details
4. User clicks "Continue to Payment"
5. **Redirected to Stripe Checkout** (secure payment page)
6. User enters card details on Stripe
7. Payment processed by Stripe
8. **Redirected back to success page**
9. Order confirmed

### Technical Flow

```
┌─────────┐         ┌──────────┐         ┌─────────────┐         ┌────────┐
│ Frontend│         │ Backend  │         │  PostgreSQL │         │ Stripe │
└────┬────┘         └────┬─────┘         └──────┬──────┘         └───┬────┘
     │                   │                       │                    │
     │ POST /checkout-   │                       │                    │
     │ session           │                       │                    │
     ├──────────────────>│                       │                    │
     │                   │ Save payment record   │                    │
     │                   ├──────────────────────>│                    │
     │                   │                       │                    │
     │                   │ Create checkout session                    │
     │                   ├───────────────────────────────────────────>│
     │                   │<───────────────────────────────────────────┤
     │                   │ Return session URL    │                    │
     │<──────────────────┤                       │                    │
     │                   │                       │                    │
     │ Redirect to       │                       │                    │
     │ Stripe Checkout   │                       │                    │
     ├───────────────────────────────────────────────────────────────>│
     │                   │                       │                    │
     │                   │                       │   User pays on     │
     │                   │                       │   Stripe page      │
     │                   │                       │                    │
     │                   │  Webhook: payment.succeeded                │
     │                   │<───────────────────────────────────────────┤
     │                   │ Update payment status │                    │
     │                   ├──────────────────────>│                    │
     │                   │                       │                    │
     │<───────────────────────────────────────────────────────────────┤
     │ Redirect to       │                       │                    │
     │ success page      │                       │                    │
     │                   │                       │                    │
     │ GET /verify/      │                       │                    │
     │ {sessionId}       │                       │                    │
     ├──────────────────>│                       │                    │
     │                   │ Fetch payment status  │                    │
     │                   ├──────────────────────>│                    │
     │                   │<──────────────────────┤                    │
     │<──────────────────┤                       │                    │
     │ Show success      │                       │                    │
     │                   │                       │                    │
```

## API Reference

### Create Checkout Session

**Endpoint:** `POST /api/payments/checkout-session`

**Request:**
```json
{
  "orderId": "uuid",
  "userId": "uuid",
  "lineItems": [
    {
      "name": "Item name",
      "description": "Item description",
      "unitPrice": 9.99,
      "quantity": 2,
      "imageUrl": "https://example.com/image.jpg"
    }
  ],
  "successUrl": "http://localhost:3000/checkout/success?session_id={CHECKOUT_SESSION_ID}",
  "cancelUrl": "http://localhost:3000/checkout",
  "idempotencyKey": "unique-key"
}
```

**Response:**
```json
{
  "sessionId": "cs_test_...",
  "sessionUrl": "https://checkout.stripe.com/c/pay/cs_test_...",
  "paymentId": "uuid",
  "message": "Checkout session created successfully"
}
```

### Verify Payment

**Endpoint:** `GET /api/payments/verify/{sessionId}`

**Response:**
```json
{
  "id": "uuid",
  "orderId": "uuid",
  "userId": "uuid",
  "amount": 19.98,
  "currency": "USD",
  "provider": 0,
  "providerPaymentId": "cs_test_...",
  "status": 1,
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-01T00:00:00Z"
}
```

## Testing

### Test Card Numbers

Use these card numbers in Stripe test mode:

| Card Number          | Scenario                    |
|---------------------|-----------------------------|
| 4242 4242 4242 4242 | Successful payment          |
| 4000 0000 0000 0002 | Card declined               |
| 4000 0025 0000 3155 | Requires authentication     |
| 4000 0000 0000 9995 | Insufficient funds          |

- Use any future expiry date (e.g., 12/34)
- Use any 3-digit CVC (e.g., 123)
- Use any valid ZIP code (e.g., 12345)

### Manual Testing Steps

1. **Add items to cart**
   - Go to http://localhost:3000/menu
   - Add some items to cart

2. **Start checkout**
   - Go to cart and click "Checkout"
   - Fill in pickup location and time
   - Click "Continue to Payment"

3. **Complete payment on Stripe**
   - You'll be redirected to Stripe Checkout
   - Enter test card: 4242 4242 4242 4242
   - Enter any email, expiry (12/34), and CVC (123)
   - Click "Pay"

4. **Verify success**
   - You'll be redirected back to success page
   - Payment should be verified and displayed
   - Cart should be cleared

5. **Check backend logs**
   - Backend should show webhook received
   - Payment status updated to "Succeeded"

### Webhook Testing

```bash
# Start webhook listener
stripe listen --forward-to localhost:5001/api/payments/webhook

# Trigger test events
stripe trigger payment_intent.succeeded
stripe trigger checkout.session.completed
```

## Production Deployment

### 1. Get Production API Keys

1. Activate your Stripe account
2. Switch to Live mode
3. Get production API keys (starts with `pk_live_` and `sk_live_`)

### 2. Configure Production Environment

```env
# Production .env
Stripe__SecretKey=sk_live_...
Stripe__PublishableKey=pk_live_...
Stripe__WebhookSecret=whsec_... (from production webhook)
```

### 3. Setup Production Webhook

1. Go to Stripe Dashboard > Developers > Webhooks
2. Add endpoint: `https://your-api-domain.com/api/payments/webhook`
3. Select events:
   - `checkout.session.completed`
   - `payment_intent.succeeded`
   - `payment_intent.payment_failed`
4. Save webhook secret to environment variables

### 4. Update Frontend URLs

```env
NEXT_PUBLIC_PAYMENT_API_URL=https://your-api-domain.com/api
NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY=pk_live_...
```

## Troubleshooting

### Payment fails immediately

- Check Stripe API keys are correct
- Verify backend is running on port 5001
- Check browser console for errors

### Webhook not received

- Ensure Stripe CLI is running (`stripe listen`)
- Verify webhook secret is correct
- Check backend logs for webhook requests

### "Authentication required" error

- Ensure user is signed in with Google OAuth
- Check session is valid

### Database connection error

- Verify PostgreSQL is running
- Check connection string in backend config
- Run database migrations if needed

## Security Best Practices

1. **Never expose secret keys**
   - Keep `sk_` keys server-side only
   - Only use `pk_` keys in frontend
   - Never commit keys to git

2. **Validate webhooks**
   - Always verify webhook signatures
   - Check webhook source is Stripe

3. **Use HTTPS in production**
   - All API calls must use HTTPS
   - Stripe requires HTTPS for webhooks

4. **Implement idempotency**
   - Always send idempotency keys
   - Prevents duplicate charges

5. **Handle errors gracefully**
   - Show user-friendly error messages
   - Log errors for debugging
   - Retry failed requests

## Additional Resources

- [Stripe Documentation](https://stripe.com/docs)
- [Stripe Checkout Guide](https://stripe.com/docs/payments/checkout)
- [Stripe Webhooks](https://stripe.com/docs/webhooks)
- [Stripe Testing](https://stripe.com/docs/testing)
- [Stripe CLI](https://stripe.com/docs/stripe-cli)

## Support

For issues or questions:
1. Check Stripe Dashboard for payment details
2. Review backend logs for errors
3. Test with Stripe CLI webhook listener
4. Consult Stripe documentation

