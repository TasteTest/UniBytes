# Orders Service — Product Backlog

## Overview
The Orders service manages customer orders, order items, and order lifecycle (from placement to completion/cancellation).

---

## Epic 1: Order Placement

### User Story 1.1: Create New Order
**As a** customer  
**I want to** place an order with multiple items  
**So that** I can purchase food from the cafeteria

**Acceptance Criteria:**
- Customer can create order with one or more order items
- Each item includes: menu_item_id, name, quantity, unit_price, modifiers
- Order total is calculated automatically
- Order status is set to Pending (0) by default
- Payment status is set to NotPaid (0) by default
- Placed_at timestamp is recorded

**Tasks:**
- [ ] Create POST /api/orders endpoint
- [ ] Implement CreateOrderCommand with MediatR
- [ ] Add FluentValidation (at least 1 item, quantity > 0, price > 0)
- [ ] Calculate order total from items
- [ ] Add user authorization check
- [ ] Return created order with ID
- [ ] Write unit tests for order creation

**Priority:** P0 (Critical)  
**Story Points:** 5

---

### User Story 1.2: Add Items to Order
**As a** customer  
**I want to** add items to my cart before placing order  
**So that** I can order multiple items at once

**Acceptance Criteria:**
- Customer can add multiple items with quantities
- Items can have modifiers (extras, customizations)
- Order total is recalculated after each addition
- Cart persists until order is placed

**Tasks:**
- [ ] Create POST /api/orders/draft endpoint for draft orders
- [ ] Implement AddOrderItemCommand
- [ ] Store draft orders temporarily (in session or DB)
- [ ] Calculate running total
- [ ] Write unit tests for adding items

**Priority:** P0 (Critical)  
**Story Points:** 5

---

## Epic 2: Order Management

### User Story 2.1: View Order Details
**As a** customer  
**I want to** view my order details  
**So that** I can see what I ordered and track its status

**Acceptance Criteria:**
- Customer can view full order details by order ID
- Details include: items, quantities, prices, total, status, placed_at
- Customer can only view their own orders (unless admin)

**Tasks:**
- [ ] Create GET /api/orders/{id} endpoint
- [ ] Implement GetOrderByIdQuery
- [ ] Add authorization check (user_id match or admin)
- [ ] Return order with related order_items
- [ ] Write unit tests for order retrieval

**Priority:** P0 (Critical)  
**Story Points:** 3

---

### User Story 2.2: List My Orders (Order History)
**As a** customer  
**I want to** view my order history  
**So that** I can see my past orders

**Acceptance Criteria:**
- Customer can view paginated list of their orders
- Orders are sorted by placed_at (newest first)
- Each order shows: id, total, status, placed_at
- Orders can be filtered by status

**Tasks:**
- [ ] Create GET /api/orders endpoint
- [ ] Implement GetOrdersQuery with user_id filter
- [ ] Add pagination (default 20 per page)
- [ ] Support status filtering
- [ ] Order by placed_at DESC
- [ ] Write unit tests for order listing

**Priority:** P0 (Critical)  
**Story Points:** 3

---

### User Story 2.3: Update Order Status
**As a** kitchen staff or admin  
**I want to** update order status  
**So that** customers can track their order progress

**Acceptance Criteria:**
- Status can be updated through: Pending → Confirmed → Preparing → Ready → Completed
- Invalid status transitions are rejected
- Updated_at timestamp is set
- Customer is notified of status changes (via frontend polling or webhook)

**Tasks:**
- [ ] Create PATCH /api/orders/{id}/status endpoint
- [ ] Implement UpdateOrderStatusCommand
- [ ] Add status transition validation
- [ ] Add admin/kitchen authorization check
- [ ] Emit event/notification for status change
- [ ] Write unit tests for status updates

**Priority:** P0 (Critical)  
**Story Points:** 5

---

### User Story 2.4: Cancel Order
**As a** customer  
**I want to** cancel my pending order  
**So that** I can change my mind before preparation starts

**Acceptance Criteria:**
- Customer can cancel orders in Pending or Confirmed status only
- Once Preparing, order cannot be cancelled by customer
- Cancel_requested_at timestamp is recorded
- Status is updated to Cancelled (5)
- Canceled_at timestamp is recorded
- Refund is initiated if payment was already made

**Tasks:**
- [ ] Create POST /api/orders/{id}/cancel endpoint
- [ ] Implement CancelOrderCommand
- [ ] Check order status (must be Pending or Confirmed)
- [ ] Add user authorization check
- [ ] Update status to Cancelled
- [ ] Trigger refund process if payment made
- [ ] Write unit tests for order cancellation

**Priority:** P1 (High)  
**Story Points:** 5

---

### User Story 2.5: Reorder Previous Order
**As a** customer  
**I want to** quickly reorder a previous order  
**So that** I don't have to add items again

**Acceptance Criteria:**
- Customer can click "Reorder" on past order
- System creates new order with same items
- Customer can review and modify before placing
- Original order remains unchanged

**Tasks:**
- [ ] Create POST /api/orders/{id}/reorder endpoint
- [ ] Implement ReorderCommand
- [ ] Copy items from original order to new draft order
- [ ] Check item availability before reordering
- [ ] Return new draft order ID
- [ ] Write unit tests for reorder functionality

**Priority:** P2 (Medium)  
**Story Points:** 3

---

## Epic 3: Admin Order Management

### User Story 3.1: View All Orders (Admin)
**As an** admin  
**I want to** view all orders in the system  
**So that** I can monitor order activity

**Acceptance Criteria:**
- Admin can view paginated list of all orders
- Orders can be filtered by: status, user_id, date range
- Orders can be sorted by: placed_at, total_amount
- Search by order ID supported

**Tasks:**
- [ ] Add admin view to GET /api/orders endpoint
- [ ] Implement admin authorization check
- [ ] Add filtering by status, user_id, date range
- [ ] Add sorting options
- [ ] Add order ID search
- [ ] Write unit tests for admin order listing

**Priority:** P2 (Medium)  
**Story Points:** 5

---

### User Story 3.2: Manually Update Order (Admin)
**As an** admin  
**I want to** manually update order details  
**So that** I can fix issues or make adjustments

**Acceptance Criteria:**
- Admin can update: total_amount, order_status, payment_status
- Changes are logged in metadata field
- Updated_at timestamp is set
- Success message shown

**Tasks:**
- [ ] Create PUT /api/orders/{id} endpoint
- [ ] Implement UpdateOrderCommand
- [ ] Add admin authorization check
- [ ] Log changes in metadata JSONB field
- [ ] Write unit tests for admin order update

**Priority:** P3 (Low)  
**Story Points:** 3

---

## Epic 4: Order Analytics

### User Story 4.1: View Order Statistics (Admin)
**As an** admin  
**I want to** view order statistics  
**So that** I can understand sales trends

**Acceptance Criteria:**
- Admin can view: total orders, total revenue, average order value
- Metrics can be filtered by date range
- Breakdown by order status
- Top selling items

**Tasks:**
- [ ] Create GET /api/orders/analytics endpoint
- [ ] Implement GetOrderAnalyticsQuery
- [ ] Aggregate total orders, revenue, average value
- [ ] Add date range filtering
- [ ] Add admin authorization check
- [ ] Write unit tests for analytics queries

**Priority:** P3 (Low)  
**Story Points:** 5

---

## Technical Debt & Infrastructure

### Task: Set Up EF Core DbContext
- [ ] Create OrdersDbContext with DbSets for orders, order_items
- [ ] Configure entity mappings (enums as int, JSONB for metadata/modifiers)
- [ ] Add connection string configuration
- [ ] Create initial migration

**Priority:** P0 (Critical)  
**Story Points:** 3

---

### Task: Implement Domain Events
- [ ] Create OrderCreated, OrderStatusChanged, OrderCancelled events
- [ ] Set up event publisher/subscriber pattern
- [ ] Integrate with MediatR notification pipeline
- [ ] Publish events for cross-service communication

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

### Task: Implement Idempotency
- [ ] Add idempotency key support for order creation
- [ ] Check for duplicate order submissions
- [ ] Return existing order if duplicate detected

**Priority:** P1 (High)  
**Story Points:** 3

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
