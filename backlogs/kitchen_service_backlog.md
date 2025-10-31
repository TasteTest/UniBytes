# Kitchen Service — Product Backlog

## Overview
The Kitchen service manages the kitchen queue, order preparation tracking, and station-based workflows for cafeteria staff.

---

## Epic 1: Kitchen Queue Management

### User Story 1.1: View Kitchen Queue
**As a** kitchen staff member  
**I want to** see all pending orders in the kitchen queue  
**So that** I can prioritize and prepare orders

**Acceptance Criteria:**
- Kitchen dashboard shows all orders with status Queued or InProgress
- Orders display: order_id, station, items, placed_at, status
- Orders are sorted by placed_at (oldest first)
- Queue updates in near real-time

**Tasks:**
- [ ] Create GET /api/kitchen/queue endpoint
- [ ] Implement GetKitchenQueueQuery
- [ ] Filter by status (Queued, InProgress)
- [ ] Order by created_at ASC
- [ ] Add kitchen staff authorization check
- [ ] Return kitchen_orders with related kitchen_items
- [ ] Write unit tests for queue retrieval

**Priority:** P0 (Critical)  
**Story Points:** 3

---

### User Story 1.2: Create Kitchen Order from Order Service
**As a** system  
**I want to** automatically create kitchen orders when orders are confirmed  
**So that** kitchen staff can see new orders immediately

**Acceptance Criteria:**
- When order status changes to Confirmed, create kitchen_order
- Kitchen order includes all order items
- Status is set to Queued (0)
- Order is assigned to appropriate station (if configured)
- Kitchen items are created for each order item

**Tasks:**
- [ ] Create POST /api/kitchen/orders endpoint (internal)
- [ ] Implement CreateKitchenOrderCommand
- [ ] Listen to OrderConfirmed event from Orders service
- [ ] Map order items to kitchen items
- [ ] Assign station based on item type (if applicable)
- [ ] Write unit tests for kitchen order creation

**Priority:** P0 (Critical)  
**Story Points:** 5

---

### User Story 1.3: Filter Queue by Station
**As a** kitchen staff member  
**I want to** filter the queue by my station  
**So that** I only see orders relevant to my work area

**Acceptance Criteria:**
- Kitchen queue can be filtered by station (Grill, Salad, Drinks, etc.)
- Filter is optional (default shows all stations)
- Station names are predefined or configurable

**Tasks:**
- [ ] Add station parameter to GET /api/kitchen/queue
- [ ] Implement station filtering in GetKitchenQueueQuery
- [ ] Support multiple station selections
- [ ] Write unit tests for station filtering

**Priority:** P1 (High)  
**Story Points:** 2

---

## Epic 2: Order Preparation Tracking

### User Story 2.1: Claim/Start Order Preparation
**As a** kitchen staff member  
**I want to** claim an order and start preparing it  
**So that** others know I'm working on it

**Acceptance Criteria:**
- Staff can claim order from queue
- Order status changes to InProgress (1)
- Started_at timestamp is recorded
- Assigned_to field is set to staff member ID/name
- Order moves to "In Progress" section of dashboard

**Tasks:**
- [ ] Create POST /api/kitchen/orders/{id}/start endpoint
- [ ] Implement StartKitchenOrderCommand
- [ ] Update status to InProgress
- [ ] Set assigned_to and started_at
- [ ] Add kitchen staff authorization check
- [ ] Emit OrderStarted event
- [ ] Write unit tests for order start

**Priority:** P0 (Critical)  
**Story Points:** 3

---

### User Story 2.2: Mark Item as Ready
**As a** kitchen staff member  
**I want to** mark individual items as ready  
**So that** customers know which parts of their order are complete

**Acceptance Criteria:**
- Staff can mark each kitchen_item as Ready (2)
- Item status changes independently
- When all items are ready, order status changes to Ready (2)
- Ready_at timestamp is recorded for each item

**Tasks:**
- [ ] Create PATCH /api/kitchen/items/{id}/status endpoint
- [ ] Implement UpdateKitchenItemStatusCommand
- [ ] Update item status to Ready
- [ ] Check if all items are ready, update kitchen_order status
- [ ] Set ready_at timestamp
- [ ] Write unit tests for item status update

**Priority:** P0 (Critical)  
**Story Points:** 3

---

### User Story 2.3: Mark Order as Complete
**As a** kitchen staff member  
**I want to** mark order as completed  
**So that** it's removed from my queue

**Acceptance Criteria:**
- Staff can mark order as Completed (3)
- Completed_at timestamp is recorded
- Order is removed from active queue
- Order service is notified to update order status to Ready/Completed

**Tasks:**
- [ ] Create POST /api/kitchen/orders/{id}/complete endpoint
- [ ] Implement CompleteKitchenOrderCommand
- [ ] Update status to Completed
- [ ] Set completed_at timestamp
- [ ] Emit OrderCompleted event (notify Orders service)
- [ ] Write unit tests for order completion

**Priority:** P0 (Critical)  
**Story Points:** 3

---

### User Story 2.4: Cancel Kitchen Order
**As a** kitchen staff member  
**I want to** cancel a kitchen order  
**So that** I can handle customer cancellations

**Acceptance Criteria:**
- Staff can cancel order if not yet completed
- Status changes to Cancelled (4)
- Order is removed from queue
- Cancellation reason can be added as note

**Tasks:**
- [ ] Create POST /api/kitchen/orders/{id}/cancel endpoint
- [ ] Implement CancelKitchenOrderCommand
- [ ] Update status to Cancelled
- [ ] Add optional cancellation note
- [ ] Emit OrderCancelled event
- [ ] Write unit tests for order cancellation

**Priority:** P1 (High)  
**Story Points:** 3

---

## Epic 3: Kitchen Dashboard & Monitoring

### User Story 3.1: View Kitchen Performance Metrics
**As a** kitchen manager  
**I want to** view kitchen performance metrics  
**So that** I can optimize operations

**Acceptance Criteria:**
- Dashboard shows: orders completed today, average prep time, orders in queue
- Metrics update in near real-time
- Breakdown by station
- Peak hours visualization

**Tasks:**
- [ ] Create GET /api/kitchen/metrics endpoint
- [ ] Implement GetKitchenMetricsQuery
- [ ] Calculate orders completed, average prep time, queue length
- [ ] Add date range filtering
- [ ] Add kitchen manager authorization check
- [ ] Write unit tests for metrics calculation

**Priority:** P2 (Medium)  
**Story Points:** 5

---

### User Story 3.2: View Order Preparation History
**As a** kitchen manager  
**I want to** view completed order history  
**So that** I can review past performance

**Acceptance Criteria:**
- Manager can view paginated list of completed orders
- Each order shows: order_id, prep time, staff member, completion time
- Orders can be filtered by date range and station
- Orders can be sorted by prep time

**Tasks:**
- [ ] Create GET /api/kitchen/history endpoint
- [ ] Implement GetKitchenHistoryQuery
- [ ] Filter by status = Completed
- [ ] Add date range and station filtering
- [ ] Calculate prep time (completed_at - started_at)
- [ ] Write unit tests for history retrieval

**Priority:** P2 (Medium)  
**Story Points:** 3

---

## Epic 4: Kitchen Item Notes & Special Requests

### User Story 4.1: Add Special Request Notes to Items
**As a** customer (via Orders service)  
**I want to** add special requests to my order items  
**So that** kitchen staff can accommodate my preferences

**Acceptance Criteria:**
- Special request notes are stored in kitchen_items.note field
- Notes are visible to kitchen staff in queue
- Notes are optional

**Tasks:**
- [ ] Ensure kitchen_items.note is populated from order_items
- [ ] Display notes prominently in kitchen dashboard
- [ ] Add character limit validation (e.g., 200 chars)
- [ ] Write unit tests for note handling

**Priority:** P1 (High)  
**Story Points:** 2

---

### User Story 4.2: View Item Preparation Details
**As a** kitchen staff member  
**I want to** see detailed preparation instructions for items  
**So that** I can prepare them correctly

**Acceptance Criteria:**
- Staff can click on kitchen_item to see full details
- Details include: name, quantity, modifiers, notes
- Modal or expanded view shows all information

**Tasks:**
- [ ] Create GET /api/kitchen/items/{id} endpoint
- [ ] Implement GetKitchenItemByIdQuery
- [ ] Return full item details
- [ ] Write unit tests for item detail retrieval

**Priority:** P2 (Medium)  
**Story Points:** 2

---

## Technical Debt & Infrastructure

### Task: Set Up EF Core DbContext
- [ ] Create KitchenDbContext with DbSets for kitchen_orders, kitchen_items
- [ ] Configure entity mappings (enums as int)
- [ ] Add connection string configuration
- [ ] Create initial migration

**Priority:** P0 (Critical)  
**Story Points:** 3

---

### Task: Implement Real-Time Updates
- [ ] Set up SignalR or WebSocket connection for kitchen dashboard
- [ ] Push new orders to connected clients
- [ ] Push status updates to all clients
- [ ] Handle connection/reconnection logic

**Priority:** P1 (High)  
**Story Points:** 8

---

### Task: Set Up Event Listeners
- [ ] Listen to OrderConfirmed event from Orders service
- [ ] Listen to OrderCancelled event from Orders service
- [ ] Implement event handlers to create/cancel kitchen orders
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

## Definition of Done
- [ ] Code written and follows project conventions
- [ ] Unit tests written and passing (minimum 80% coverage)
- [ ] Integration tests written for API endpoints
- [ ] FluentValidation rules implemented
- [ ] API documented (Swagger/OpenAPI)
- [ ] Code reviewed and approved
- [ ] Deployed to dev environment
- [ ] Manual QA completed
