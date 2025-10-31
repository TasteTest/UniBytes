# Menu Service — Product Backlog

## Overview
The Menu service manages menu categories, menu items, and menu item availability for the cafeteria.

---

## Epic 1: Menu Category Management

### User Story 1.1: Create Menu Category (Admin)
**As an** admin  
**I want to** create new menu categories  
**So that** I can organize menu items by type

**Acceptance Criteria:**
- Admin can create category with name, description, display_order
- Category name must be unique
- Category is active by default
- Success message shown on creation

**Tasks:**
- [ ] Create POST /api/menu/categories endpoint
- [ ] Implement CreateMenuCategoryCommand with MediatR
- [ ] Add FluentValidation (name required, unique, max length)
- [ ] Add admin authorization check
- [ ] Return created category with ID
- [ ] Write unit tests for category creation

**Priority:** P0 (Critical)  
**Story Points:** 3

---

### User Story 1.2: List Menu Categories
**As a** customer  
**I want to** view all available menu categories  
**So that** I can browse items by category

**Acceptance Criteria:**
- Endpoint returns all active categories
- Categories are sorted by display_order
- Each category shows: id, name, description
- Response is cached for performance

**Tasks:**
- [ ] Create GET /api/menu/categories endpoint
- [ ] Implement GetMenuCategoriesQuery with MediatR
- [ ] Filter by is_active = true
- [ ] Order by display_order
- [ ] Add response caching
- [ ] Write unit tests for category listing

**Priority:** P0 (Critical)  
**Story Points:** 2

---

### User Story 1.3: Update Menu Category (Admin)
**As an** admin  
**I want to** update menu category details  
**So that** I can keep category information current

**Acceptance Criteria:**
- Admin can update: name, description, display_order, is_active
- Name uniqueness validated
- Updated_at timestamp is set
- Success message shown

**Tasks:**
- [ ] Create PUT /api/menu/categories/{id} endpoint
- [ ] Implement UpdateMenuCategoryCommand
- [ ] Add FluentValidation
- [ ] Add admin authorization check
- [ ] Return updated category
- [ ] Write unit tests for category update

**Priority:** P1 (High)  
**Story Points:** 3

---

### User Story 1.4: Delete Menu Category (Admin)
**As an** admin  
**I want to** delete unused menu categories  
**So that** I can keep the menu organized

**Acceptance Criteria:**
- Admin can delete category if it has no items
- Confirmation required before deletion
- If category has items, show error message
- Hard delete from database

**Tasks:**
- [ ] Create DELETE /api/menu/categories/{id} endpoint
- [ ] Implement DeleteMenuCategoryCommand
- [ ] Check for existing menu items before deletion
- [ ] Add admin authorization check
- [ ] Write unit tests for category deletion

**Priority:** P2 (Medium)  
**Story Points:** 3

---

## Epic 2: Menu Item Management

### User Story 2.1: Create Menu Item (Admin)
**As an** admin  
**I want to** add new menu items  
**So that** customers can order them

**Acceptance Criteria:**
- Admin can create item with: name, description, price, category_id, currency
- Price must be greater than 0
- Item is available and public by default
- Success message shown

**Tasks:**
- [ ] Create POST /api/menu/items endpoint
- [ ] Implement CreateMenuItemCommand
- [ ] Add FluentValidation (name required, price > 0, category exists)
- [ ] Add admin authorization check
- [ ] Return created item with ID
- [ ] Write unit tests for item creation

**Priority:** P0 (Critical)  
**Story Points:** 3

---

### User Story 2.2: List Menu Items (Public)
**As a** customer  
**I want to** browse all available menu items  
**So that** I can decide what to order

**Acceptance Criteria:**
- Endpoint returns all available and public items
- Items can be filtered by category_id
- Items show: id, name, description, price, category
- Results are paginated
- Response is cached

**Tasks:**
- [ ] Create GET /api/menu/items endpoint
- [ ] Implement GetMenuItemsQuery
- [ ] Filter by available = true and visibility = 0 (Public)
- [ ] Support category_id filter
- [ ] Add pagination (default 20 per page)
- [ ] Add response caching
- [ ] Write unit tests for item listing

**Priority:** P0 (Critical)  
**Story Points:** 3

---

### User Story 2.3: View Menu Item Details
**As a** customer  
**I want to** view detailed information about a menu item  
**So that** I can make an informed ordering decision

**Acceptance Criteria:**
- Endpoint returns full item details including components (JSONB)
- Shows: name, description, price, category, availability, visibility
- If item is unavailable, show appropriate message

**Tasks:**
- [ ] Create GET /api/menu/items/{id} endpoint
- [ ] Implement GetMenuItemByIdQuery
- [ ] Return full item details including components
- [ ] Write unit tests for item retrieval

**Priority:** P0 (Critical)  
**Story Points:** 2

---

### User Story 2.4: Update Menu Item (Admin)
**As an** admin  
**I want to** update menu item information  
**So that** I can change prices, descriptions, or availability

**Acceptance Criteria:**
- Admin can update: name, description, price, category_id, available, visibility, components
- Price must be greater than 0
- Updated_at timestamp is set
- Success message shown

**Tasks:**
- [ ] Create PUT /api/menu/items/{id} endpoint
- [ ] Implement UpdateMenuItemCommand
- [ ] Add FluentValidation
- [ ] Add admin authorization check
- [ ] Invalidate cache after update
- [ ] Return updated item
- [ ] Write unit tests for item update

**Priority:** P1 (High)  
**Story Points:** 3

---

### User Story 2.5: Mark Item as Unavailable (Admin)
**As an** admin  
**I want to** mark items as unavailable  
**So that** customers cannot order out-of-stock items

**Acceptance Criteria:**
- Admin can toggle available flag
- Unavailable items are hidden from public listing
- Customers cannot add unavailable items to cart
- Success message shown

**Tasks:**
- [ ] Create PATCH /api/menu/items/{id}/availability endpoint
- [ ] Implement ToggleMenuItemAvailabilityCommand
- [ ] Add admin authorization check
- [ ] Invalidate cache after update
- [ ] Write unit tests for availability toggle

**Priority:** P1 (High)  
**Story Points:** 2

---

### User Story 2.6: Delete Menu Item (Admin)
**As an** admin  
**I want to** delete menu items  
**So that** I can remove discontinued items

**Acceptance Criteria:**
- Admin can delete any menu item
- Confirmation required before deletion
- Soft delete preferred (set is_active = false)
- Success message shown

**Tasks:**
- [ ] Create DELETE /api/menu/items/{id} endpoint
- [ ] Implement DeleteMenuItemCommand
- [ ] Add admin authorization check
- [ ] Invalidate cache after deletion
- [ ] Write unit tests for item deletion

**Priority:** P2 (Medium)  
**Story Points:** 2

---

## Epic 3: Menu Search & Filtering

### User Story 3.1: Search Menu Items by Name
**As a** customer  
**I want to** search for menu items by name  
**So that** I can quickly find what I want to order

**Acceptance Criteria:**
- Search endpoint accepts query parameter
- Results match items containing search term (case-insensitive)
- Only available and public items are returned
- Results are paginated

**Tasks:**
- [ ] Add search parameter to GET /api/menu/items
- [ ] Implement search logic in GetMenuItemsQuery
- [ ] Use case-insensitive LIKE/ILIKE query
- [ ] Write unit tests for search functionality

**Priority:** P1 (High)  
**Story Points:** 3

---

### User Story 3.2: Filter Menu Items by Price Range
**As a** customer  
**I want to** filter menu items by price range  
**So that** I can find items within my budget

**Acceptance Criteria:**
- Filter accepts min_price and max_price parameters
- Results include only items within price range
- Parameters are optional

**Tasks:**
- [ ] Add min_price/max_price parameters to GET /api/menu/items
- [ ] Implement price range filtering in GetMenuItemsQuery
- [ ] Add validation for price parameters (>= 0)
- [ ] Write unit tests for price filtering

**Priority:** P2 (Medium)  
**Story Points:** 2

---

## Technical Debt & Infrastructure

### Task: Set Up EF Core DbContext
- [ ] Create MenuDbContext with DbSets for menu_categories, menu_items
- [ ] Configure entity mappings (enums as int, JSONB for components)
- [ ] Add connection string configuration
- [ ] Create initial migration

**Priority:** P0 (Critical)  
**Story Points:** 3

---

### Task: Implement Caching Strategy
- [ ] Set up distributed cache (Redis or in-memory)
- [ ] Cache GET /api/menu/categories response
- [ ] Cache GET /api/menu/items response
- [ ] Invalidate cache on item/category updates
- [ ] Configure cache expiration times

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

## Definition of Done
- [ ] Code written and follows project conventions
- [ ] Unit tests written and passing (minimum 80% coverage)
- [ ] Integration tests written for API endpoints
- [ ] FluentValidation rules implemented
- [ ] API documented (Swagger/OpenAPI)
- [ ] Code reviewed and approved
- [ ] Deployed to dev environment
- [ ] Manual QA completed
