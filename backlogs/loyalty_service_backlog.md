# Loyalty Service — Product Backlog

## Overview
The Loyalty service manages customer loyalty accounts, points earning, points redemption, and tier management.

---

## Epic 1: Loyalty Account Management

### User Story 1.1: Create Loyalty Account
**As a** new customer  
**I want to** automatically get a loyalty account  
**So that** I can start earning points on my orders

**Acceptance Criteria:**
- Loyalty account is created automatically when user signs up
- Account starts with 0 points and Bronze tier (0)
- Account is active by default
- User_id is unique per loyalty account

**Tasks:**
- [ ] Create POST /api/loyalty/accounts endpoint (internal)
- [ ] Implement CreateLoyaltyAccountCommand
- [ ] Listen to UserCreated event from Users service
- [ ] Set initial points_balance to 0
- [ ] Set initial tier to Bronze (0)
- [ ] Write unit tests for account creation

**Priority:** P0 (Critical)  
**Story Points:** 3

---

### User Story 1.2: View My Loyalty Account
**As a** customer  
**I want to** view my loyalty account details  
**So that** I can see my points balance and tier

**Acceptance Criteria:**
- Customer can view their loyalty account
- Details include: points_balance, tier, is_active
- Account is read-only for customers
- Displays tier name (Bronze/Silver/Gold)

**Tasks:**
- [ ] Create GET /api/loyalty/accounts/{user_id} endpoint
- [ ] Implement GetLoyaltyAccountQuery
- [ ] Add authorization check (user can only view own account)
- [ ] Map tier integer to tier name
- [ ] Write unit tests for account retrieval

**Priority:** P0 (Critical)  
**Story Points:** 2

---

### User Story 1.3: Deactivate Loyalty Account (Admin)
**As an** admin  
**I want to** deactivate a loyalty account  
**So that** I can handle account issues

**Acceptance Criteria:**
- Admin can deactivate any loyalty account
- Points balance is preserved
- Deactivated accounts cannot earn or redeem points
- Confirmation required before deactivation

**Tasks:**
- [ ] Create POST /api/loyalty/accounts/{id}/deactivate endpoint
- [ ] Implement DeactivateLoyaltyAccountCommand
- [ ] Set is_active to false
- [ ] Add admin authorization check
- [ ] Write unit tests for deactivation

**Priority:** P2 (Medium)  
**Story Points:** 2

---

## Epic 2: Points Earning

### User Story 2.1: Earn Points on Order Completion
**As a** customer  
**I want to** earn points when my order is completed  
**So that** I can accumulate rewards

**Acceptance Criteria:**
- Customer earns 1 point per $1 spent (configurable ratio)
- Points are added when order status is Completed
- Transaction record is created with reason "earn"
- Points balance is updated automatically
- Customer is notified of points earned

**Tasks:**
- [ ] Create POST /api/loyalty/accounts/{id}/earn endpoint (internal)
- [ ] Implement EarnPointsCommand
- [ ] Listen to OrderCompleted event from Orders service
- [ ] Calculate points based on order total
- [ ] Create loyalty_transaction record
- [ ] Update points_balance
- [ ] Write unit tests for points earning

**Priority:** P0 (Critical)  
**Story Points:** 5

---

### User Story 2.2: View Points Transaction History
**As a** customer  
**I want to** view my points transaction history  
**So that** I can see how I earned and spent points

**Acceptance Criteria:**
- Customer can view paginated list of transactions
- Each transaction shows: change_amount, reason, reference_id, created_at
- Transactions are sorted by created_at (newest first)
- Positive amounts are earnings, negative are redemptions

**Tasks:**
- [ ] Create GET /api/loyalty/transactions endpoint
- [ ] Implement GetLoyaltyTransactionsQuery
- [ ] Filter by loyalty_account_id
- [ ] Add pagination (default 20 per page)
- [ ] Order by created_at DESC
- [ ] Write unit tests for transaction listing

**Priority:** P1 (High)  
**Story Points:** 3

---

### User Story 2.3: Manual Points Adjustment (Admin)
**As an** admin  
**I want to** manually adjust customer points  
**So that** I can fix errors or provide goodwill gestures

**Acceptance Criteria:**
- Admin can add or subtract points
- Adjustment reason is required
- Transaction record is created with reason "adjust"
- Points balance is updated
- Customer is notified of adjustment

**Tasks:**
- [ ] Create POST /api/loyalty/accounts/{id}/adjust endpoint
- [ ] Implement AdjustPointsCommand
- [ ] Add FluentValidation (reason required, amount != 0)
- [ ] Create loyalty_transaction record
- [ ] Update points_balance
- [ ] Add admin authorization check
- [ ] Write unit tests for points adjustment

**Priority:** P2 (Medium)  
**Story Points:** 3

---

## Epic 3: Points Redemption

### User Story 3.1: View Available Rewards
**As a** customer  
**I want to** see what rewards I can redeem  
**So that** I can use my points

**Acceptance Criteria:**
- Customer can view list of available rewards
- Each reward shows: name, points_required, description
- Rewards customer can afford are highlighted
- Rewards are sorted by points_required

**Tasks:**
- [ ] Create GET /api/loyalty/rewards endpoint
- [ ] Implement GetAvailableRewardsQuery
- [ ] Return hardcoded or configurable reward list
- [ ] Compare points_required to customer's balance
- [ ] Write unit tests for rewards listing

**Priority:** P1 (High)  
**Story Points:** 3

---

### User Story 3.2: Redeem Points for Reward
**As a** customer  
**I want to** redeem my points for a reward  
**So that** I can get discounts or free items

**Acceptance Criteria:**
- Customer can redeem points if balance is sufficient
- Points are deducted from balance
- Redemption record is created
- Transaction record is created with reason "redeem"
- Customer receives reward code or confirmation

**Tasks:**
- [ ] Create POST /api/loyalty/redemptions endpoint
- [ ] Implement RedeemPointsCommand
- [ ] Check points_balance >= points_used
- [ ] Create loyalty_redemptions record
- [ ] Create loyalty_transaction with negative amount
- [ ] Update points_balance
- [ ] Return reward details or code
- [ ] Write unit tests for redemption

**Priority:** P1 (High)  
**Story Points:** 5

---

### User Story 3.3: View Redemption History
**As a** customer  
**I want to** view my redemption history  
**So that** I can see what rewards I've claimed

**Acceptance Criteria:**
- Customer can view paginated list of redemptions
- Each redemption shows: points_used, reward_type, reward_metadata, created_at
- Redemptions are sorted by created_at (newest first)

**Tasks:**
- [ ] Create GET /api/loyalty/redemptions endpoint
- [ ] Implement GetLoyaltyRedemptionsQuery
- [ ] Filter by loyalty_account_id
- [ ] Add pagination (default 20 per page)
- [ ] Order by created_at DESC
- [ ] Write unit tests for redemption listing

**Priority:** P2 (Medium)  
**Story Points:** 3

---

### User Story 3.4: Cancel Redemption (Admin)
**As an** admin  
**I want to** cancel a redemption and refund points  
**So that** I can handle issues

**Acceptance Criteria:**
- Admin can cancel any redemption
- Points are refunded to customer balance
- Transaction record is created with reason "refund"
- Redemption record is marked as cancelled (if tracking status)

**Tasks:**
- [ ] Create POST /api/loyalty/redemptions/{id}/cancel endpoint
- [ ] Implement CancelRedemptionCommand
- [ ] Refund points to loyalty account
- [ ] Create loyalty_transaction with positive amount
- [ ] Add admin authorization check
- [ ] Write unit tests for redemption cancellation

**Priority:** P3 (Low)  
**Story Points:** 3

---

## Epic 4: Tier Management

### User Story 4.1: Auto-Upgrade Tier Based on Points
**As a** customer  
**I want to** automatically advance to higher tiers  
**So that** I can get better rewards

**Acceptance Criteria:**
- Tiers: Bronze (0), Silver (1), Gold (2)
- Tier thresholds: Bronze (0-499 pts), Silver (500-1499 pts), Gold (1500+ pts)
- Tier is updated automatically when points balance changes
- Customer is notified of tier changes

**Tasks:**
- [ ] Create tier calculation logic
- [ ] Implement UpdateTierCommand
- [ ] Trigger tier update on points balance change
- [ ] Update tier field in loyalty_accounts
- [ ] Emit TierChanged event for notifications
- [ ] Write unit tests for tier calculation

**Priority:** P1 (High)  
**Story Points:** 5

---

### User Story 4.2: View Tier Benefits
**As a** customer  
**I want to** see what benefits each tier provides  
**So that** I'm motivated to earn more points

**Acceptance Criteria:**
- Customer can view tier benefits page
- Shows benefits for each tier
- Shows current tier and points to next tier
- Benefits are configurable

**Tasks:**
- [ ] Create GET /api/loyalty/tiers endpoint
- [ ] Return hardcoded or configurable tier information
- [ ] Calculate points needed for next tier
- [ ] Write unit tests for tier info retrieval

**Priority:** P2 (Medium)  
**Story Points:** 2

---

## Epic 5: Loyalty Analytics

### User Story 5.1: View Loyalty Statistics (Admin)
**As an** admin  
**I want to** view loyalty program statistics  
**So that** I can measure program success

**Acceptance Criteria:**
- Admin can view: total accounts, total points issued, total redemptions
- Metrics can be filtered by date range
- Breakdown by tier
- Average points per customer

**Tasks:**
- [ ] Create GET /api/loyalty/analytics endpoint
- [ ] Implement GetLoyaltyAnalyticsQuery
- [ ] Aggregate accounts, points, redemptions
- [ ] Calculate averages and breakdowns
- [ ] Add date range filtering
- [ ] Add admin authorization check
- [ ] Write unit tests for analytics queries

**Priority:** P3 (Low)  
**Story Points:** 5

---

## Technical Debt & Infrastructure

### Task: Set Up EF Core DbContext
- [ ] Create LoyaltyDbContext with DbSets for loyalty_accounts, loyalty_transactions, loyalty_redemptions
- [ ] Configure entity mappings (enums as int, JSONB for metadata)
- [ ] Add connection string configuration
- [ ] Create initial migration

**Priority:** P0 (Critical)  
**Story Points:** 3

---

### Task: Implement Domain Events
- [ ] Create PointsEarned, PointsRedeemed, TierChanged events
- [ ] Set up event publisher/subscriber pattern
- [ ] Integrate with MediatR notification pipeline
- [ ] Publish events for notifications

**Priority:** P1 (High)  
**Story Points:** 3

---

### Task: Set Up Event Listeners
- [ ] Listen to UserCreated event from Users service
- [ ] Listen to OrderCompleted event from Orders service
- [ ] Implement event handlers to create accounts and earn points
- [ ] Add retry logic for failed event processing

**Priority:** P0 (Critical)  
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

### Task: Implement Concurrency Control
- [ ] Add optimistic concurrency for points_balance updates
- [ ] Use row versioning or timestamps
- [ ] Handle concurrent transaction conflicts
- [ ] Retry failed transactions

**Priority:** P1 (High)  
**Story Points:** 5

---

## Definition of Done
- [ ] Code written and follows project conventions
- [ ] Unit tests written and passing (minimum 80% coverage)
- [ ] Integration tests written for API endpoints
- [ ] FluentValidation rules implemented
- [ ] API documented (Swagger/OpenAPI)
- [ ] Code reviewed and approved
- [ ] Deployed to dev environment
- [ ] Manual QA completed
