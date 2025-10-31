# Payments Service — Product Backlog

## Overview
The Payments service manages payment processing via Stripe, idempotency, and payment lifecycle tracking.

---

## Epic 1: Payment Processing

### User Story 1.1: Create Payment Intent (Stripe)
**As a** customer  
**I want to** pay for my order using Stripe  
**So that** I can complete my purchase securely

**Acceptance Criteria:**
- System creates Stripe PaymentIntent when customer proceeds to checkout
- Payment includes: order_id, user_id, amount, currency
- Client secret is returned to frontend for Stripe.js
- Payment status is set to "processing"
- Idempotency key prevents duplicate payments

**Tasks:**
- [ ] Create POST /api/payments endpoint
- [ ] Implement CreatePaymentCommand with MediatR
- [ ] Integrate Stripe SDK (create PaymentIntent)
- [ ] Store payment record with provider_payment_id
- [ ] Add FluentValidation (amount > 0, order_id exists)
- [ ] Implement idempotency key checking
- [ ] Return client_secret to frontend
- [ ] Write unit tests for payment creation

**Priority:** P0 (Critical)  
**Story Points:** 8

---

### User Story 1.2: Confirm Payment Success
**As a** customer  
**I want to** receive confirmation when my payment succeeds  
**So that** I know my order is confirmed

**Acceptance Criteria:**
- System updates payment status to "succeeded" on confirmation
- Order service is notified to update order payment_status
- Customer receives success message
- Payment record stores provider_charge_id

**Tasks:**
- [ ] Create POST /api/payments/{id}/confirm endpoint
- [ ] Implement ConfirmPaymentCommand
- [ ] Update payment status to "succeeded"
- [ ] Emit PaymentSucceeded event (notify Orders service)
- [ ] Store raw provider response in JSONB
- [ ] Write unit tests for payment confirmation

**Priority:** P0 (Critical)  
**Story Points:** 5

---

### User Story 1.3: Handle Payment Failure
**As a** customer  
**I want to** be notified if my payment fails  
**So that** I can try again or use a different payment method

**Acceptance Criteria:**
- System updates payment status to "failed" on failure
- Failure message is stored in failure_message field
- Customer sees clear error message
- Customer can retry payment

**Tasks:**
- [ ] Create POST /api/payments/{id}/fail endpoint
- [ ] Implement FailPaymentCommand
- [ ] Update payment status to "failed"
- [ ] Store failure_message
- [ ] Store raw provider response
- [ ] Write unit tests for payment failure

**Priority:** P0 (Critical)  
**Story Points:** 3

---

### User Story 1.4: Refund Payment
**As an** admin  
**I want to** refund a customer's payment  
**So that** I can handle cancellations and issues

**Acceptance Criteria:**
- Admin can initiate refund for succeeded payment
- Stripe refund is created via API
- Payment status is updated to "refunded"
- Order service is notified
- Refund confirmation shown

**Tasks:**
- [ ] Create POST /api/payments/{id}/refund endpoint
- [ ] Implement RefundPaymentCommand
- [ ] Integrate Stripe refund API
- [ ] Update payment status to "refunded"
- [ ] Emit PaymentRefunded event
- [ ] Add admin authorization check
- [ ] Write unit tests for refund processing

**Priority:** P1 (High)  
**Story Points:** 5

---

## Epic 2: Payment History & Tracking

### User Story 2.1: View Payment Details
**As a** customer  
**I want to** view my payment details  
**So that** I can verify my transaction

**Acceptance Criteria:**
- Customer can view payment by ID
- Details include: amount, status, order_id, created_at, provider
- Customer can only view their own payments (unless admin)

**Tasks:**
- [ ] Create GET /api/payments/{id} endpoint
- [ ] Implement GetPaymentByIdQuery
- [ ] Add authorization check (user_id match or admin)
- [ ] Return payment details (exclude sensitive provider data)
- [ ] Write unit tests for payment retrieval

**Priority:** P0 (Critical)  
**Story Points:** 2

---

### User Story 2.2: View Payment History
**As a** customer  
**I want to** view my payment history  
**So that** I can track all my transactions

**Acceptance Criteria:**
- Customer can view paginated list of their payments
- Payments are sorted by created_at (newest first)
- Each payment shows: id, amount, status, order_id, created_at
- Payments can be filtered by status

**Tasks:**
- [ ] Create GET /api/payments endpoint
- [ ] Implement GetPaymentsQuery with user_id filter
- [ ] Add pagination (default 20 per page)
- [ ] Support status filtering
- [ ] Order by created_at DESC
- [ ] Write unit tests for payment listing

**Priority:** P1 (High)  
**Story Points:** 3

---

### User Story 2.3: View All Payments (Admin)
**As an** admin  
**I want to** view all payments in the system  
**So that** I can monitor payment activity

**Acceptance Criteria:**
- Admin can view paginated list of all payments
- Payments can be filtered by: status, user_id, date range
- Payments can be sorted by: created_at, amount
- Search by payment ID or order ID supported

**Tasks:**
- [ ] Add admin view to GET /api/payments endpoint
- [ ] Implement admin authorization check
- [ ] Add filtering by status, user_id, date range
- [ ] Add sorting options
- [ ] Add payment/order ID search
- [ ] Write unit tests for admin payment listing

**Priority:** P2 (Medium)  
**Story Points:** 5

---

## Epic 3: Idempotency & Reliability

### User Story 3.1: Prevent Duplicate Payments
**As a** system  
**I want to** prevent duplicate payments for the same order  
**So that** customers are not charged multiple times

**Acceptance Criteria:**
- System checks idempotency_keys table before creating payment
- If duplicate key exists, return existing payment
- Idempotency keys expire after 24 hours
- Keys are unique per user/order combination

**Tasks:**
- [ ] Create idempotency key generation logic
- [ ] Check for existing key in CreatePaymentCommand
- [ ] Return existing payment if key matches
- [ ] Clean up expired keys (background job)
- [ ] Write unit tests for idempotency logic

**Priority:** P0 (Critical)  
**Story Points:** 5

---

### User Story 3.2: Store Idempotency Keys
**As a** system  
**I want to** store idempotency keys for payment requests  
**So that** I can safely retry failed operations

**Acceptance Criteria:**
- Idempotency key is generated for each payment request
- Key is stored with: user_id, created_at, expires_at
- Keys expire after 24 hours
- Expired keys are automatically cleaned up

**Tasks:**
- [ ] Create POST /api/payments/idempotency-keys endpoint (internal)
- [ ] Implement StoreIdempotencyKeyCommand
- [ ] Set expires_at to 24 hours from creation
- [ ] Create background job to clean expired keys
- [ ] Write unit tests for key storage

**Priority:** P0 (Critical)  
**Story Points:** 3

---

## Epic 4: Mock/Test Payments

### User Story 4.1: Support Mock Payments (Dev/Test)
**As a** developer  
**I want to** use mock payments in development  
**So that** I can test without real Stripe charges

**Acceptance Criteria:**
- System supports "mock" provider in addition to "stripe"
- Mock payments always succeed immediately
- No actual Stripe API calls are made
- Environment variable controls provider selection

**Tasks:**
- [ ] Add provider field to payment record
- [ ] Create MockPaymentService implementing IPaymentService
- [ ] Configure provider based on environment (dev/test/prod)
- [ ] Mock payments auto-succeed after creation
- [ ] Write unit tests for mock payment flow

**Priority:** P1 (High)  
**Story Points:** 3

---

### User Story 4.2: Simulate Payment Scenarios (Test)
**As a** tester  
**I want to** simulate different payment scenarios  
**So that** I can test success, failure, and refund flows

**Acceptance Criteria:**
- Test endpoint allows simulating: success, failure, pending
- Test scenarios don't affect production data
- Only available in test/dev environments

**Tasks:**
- [ ] Create POST /api/payments/test/simulate endpoint
- [ ] Implement test scenario handlers
- [ ] Add environment check (only allow in dev/test)
- [ ] Support success, failure, refund scenarios
- [ ] Write tests for each scenario

**Priority:** P2 (Medium)  
**Story Points:** 3

---

## Epic 5: Payment Analytics

### User Story 5.1: View Payment Statistics (Admin)
**As an** admin  
**I want to** view payment statistics  
**So that** I can track revenue and payment success rates

**Acceptance Criteria:**
- Admin can view: total revenue, successful payments, failed payments
- Metrics can be filtered by date range
- Success rate percentage is calculated
- Breakdown by payment provider

**Tasks:**
- [ ] Create GET /api/payments/analytics endpoint
- [ ] Implement GetPaymentAnalyticsQuery
- [ ] Aggregate total revenue, success/failure counts
- [ ] Calculate success rate
- [ ] Add date range filtering
- [ ] Add admin authorization check
- [ ] Write unit tests for analytics queries

**Priority:** P3 (Low)  
**Story Points:** 5

---

## Technical Debt & Infrastructure

### Task: Set Up Stripe SDK Integration
- [ ] Install Stripe.NET SDK
- [ ] Configure Stripe API keys (secret key, publishable key)
- [ ] Create IPaymentService interface
- [ ] Implement StripePaymentService
- [ ] Add webhook endpoint configuration
- [ ] Set up error handling and retry logic

**Priority:** P0 (Critical)  
**Story Points:** 5

---

### Task: Set Up EF Core DbContext
- [ ] Create PaymentsDbContext with DbSets for payments, idempotency_keys
- [ ] Configure entity mappings (JSONB for raw_provider_response)
- [ ] Add connection string configuration
- [ ] Create initial migration

**Priority:** P0 (Critical)  
**Story Points:** 3

---

### Task: Implement Background Jobs
- [ ] Set up Hangfire or background service
- [ ] Create job to clean expired idempotency keys
- [ ] Create job to check pending payment statuses
- [ ] Schedule jobs to run periodically

**Priority:** P1 (High)  
**Story Points:** 5

---

### Task: Set Up MediatR Pipeline
- [ ] Configure MediatR in Program.cs
- [ ] Create base Command/Query/Result types
- [ ] Set up validation pipeline behavior
- [ ] Set up logging pipeline behavior

**Priority:** P0 (Critical)  
**Story Points:** 2

---

### Task: Implement Secure Secret Management
- [ ] Store Stripe keys in environment variables or secrets manager
- [ ] Never log sensitive payment data
- [ ] Encrypt raw_provider_response if storing card details
- [ ] Add PCI compliance notes to documentation

**Priority:** P0 (Critical)  
**Story Points:** 3

---

## Definition of Done
- [ ] Code written and follows project conventions
- [ ] Unit tests written and passing (minimum 80% coverage)
- [ ] Integration tests written for API endpoints
- [ ] FluentValidation rules implemented
- [ ] API documented (Swagger/OpenAPI)
- [ ] Stripe integration tested in test mode
- [ ] Code reviewed and approved
- [ ] Deployed to dev environment
- [ ] Manual QA completed
