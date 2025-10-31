# Users Service — Product Backlog

## Overview
The Users service manages user accounts, OAuth authentication (Google via NextAuth), user analytics, and profile data.

---

## Epic 1: User Authentication & Profile Management

### User Story 1.1: Google OAuth Sign-In
**As a** customer  
**I want to** sign in with my Google account  
**So that** I can quickly access the platform without creating a new password

**Acceptance Criteria:**
- User can click "Sign in with Google" button
- OAuth flow redirects to Google consent screen
- On success, user is redirected back with session token
- User profile is created if first-time sign-in
- Session persists across page refreshes

**Tasks:**
- [ ] Set up NextAuth configuration for Google provider
- [ ] Create `/api/auth/[...nextauth]` route
- [ ] Store OAuth provider data in `oauth_providers` table
- [ ] Create user record in `users` table on first sign-in
- [ ] Implement session management
- [ ] Add error handling for OAuth failures
- [ ] Write unit tests for authentication flow

**Priority:** P0 (Critical)  
**Story Points:** 5

---

### User Story 1.2: User Profile Viewing
**As a** logged-in user  
**I want to** view my profile information  
**So that** I can verify my account details

**Acceptance Criteria:**
- User can navigate to profile page
- Profile displays: email, first name, last name, avatar, location
- Profile shows last login timestamp
- User can see if they have admin privileges

**Tasks:**
- [ ] Create GET /api/users/{id} endpoint
- [ ] Implement query handler to fetch user by ID
- [ ] Add authorization check (user can only view own profile unless admin)
- [ ] Return user data with proper serialization
- [ ] Write unit tests for profile retrieval

**Priority:** P0 (Critical)  
**Story Points:** 3

---

### User Story 1.3: User Profile Editing
**As a** logged-in user  
**I want to** edit my profile information  
**So that** I can keep my details up to date

**Acceptance Criteria:**
- User can update: first_name, last_name, bio, location, avatar_url
- Changes are validated before saving
- Success message shown on successful update
- Email cannot be changed (tied to OAuth)
- Updated_at timestamp is automatically set

**Tasks:**
- [ ] Create PUT /api/users/{id} endpoint
- [ ] Implement command handler for updating user
- [ ] Add FluentValidation rules (name length, bio length, URL format)
- [ ] Add authorization check (user can only edit own profile)
- [ ] Return updated user data
- [ ] Write unit tests for profile update

**Priority:** P1 (High)  
**Story Points:** 3

---

### User Story 1.4: User Deactivation
**As a** user  
**I want to** deactivate my account  
**So that** I can stop using the service without deleting my data

**Acceptance Criteria:**
- User can request account deactivation
- Deactivated users cannot sign in
- is_active flag is set to false
- User data is retained for potential reactivation
- Confirmation dialog required before deactivation

**Tasks:**
- [ ] Create POST /api/users/{id}/deactivate endpoint
- [ ] Implement command handler to set is_active = false
- [ ] Add authorization check
- [ ] Invalidate existing sessions on deactivation
- [ ] Write unit tests for deactivation flow

**Priority:** P2 (Medium)  
**Story Points:** 3

---

## Epic 2: User Analytics & Tracking

### User Story 2.1: Track User Activity Events
**As a** product manager  
**I want to** track user activity events  
**So that** I can understand user behavior and improve the platform

**Acceptance Criteria:**
- System logs user events: login, logout, page views, actions
- Events include: user_id, session_id, event_type, event_data, IP, user_agent
- Events are stored in `user_analytics` table
- Events do not block user actions (async logging)

**Tasks:**
- [ ] Create POST /api/analytics/events endpoint
- [ ] Implement command handler to store events
- [ ] Add background job/queue for async event processing
- [ ] Store event data as JSONB for flexibility
- [ ] Add IP address and user agent capture
- [ ] Write unit tests for event logging

**Priority:** P2 (Medium)  
**Story Points:** 5

---

### User Story 2.2: View User Activity Dashboard (Admin)
**As an** admin  
**I want to** view user activity analytics  
**So that** I can monitor platform usage

**Acceptance Criteria:**
- Admin can view aggregated user activity metrics
- Metrics include: active users, event counts, popular actions
- Data can be filtered by date range
- Results are paginated

**Tasks:**
- [ ] Create GET /api/analytics/dashboard endpoint
- [ ] Implement query to aggregate analytics data
- [ ] Add admin authorization check
- [ ] Support date range filtering
- [ ] Return paginated results
- [ ] Write unit tests for analytics queries

**Priority:** P3 (Low)  
**Story Points:** 5

---

## Epic 3: Admin User Management

### User Story 3.1: List All Users (Admin)
**As an** admin  
**I want to** view a list of all users  
**So that** I can manage user accounts

**Acceptance Criteria:**
- Admin can view paginated list of all users
- List shows: email, name, is_active, is_admin, created_at
- Users can be filtered by is_active status
- Users can be searched by email or name

**Tasks:**
- [ ] Create GET /api/users endpoint
- [ ] Implement query handler with pagination
- [ ] Add filtering by is_active
- [ ] Add search by email/name
- [ ] Add admin authorization check
- [ ] Write unit tests for user listing

**Priority:** P2 (Medium)  
**Story Points:** 5

---

### User Story 3.2: Promote User to Admin
**As an** admin  
**I want to** promote a user to admin role  
**So that** they can help manage the platform

**Acceptance Criteria:**
- Admin can set is_admin = true for any user
- Confirmation required before promotion
- Audit log entry created for admin promotion
- Success message shown

**Tasks:**
- [ ] Create POST /api/users/{id}/promote endpoint
- [ ] Implement command handler to set is_admin = true
- [ ] Add admin authorization check
- [ ] Log admin action in user_analytics
- [ ] Write unit tests for user promotion

**Priority:** P3 (Low)  
**Story Points:** 3

---

## Technical Debt & Infrastructure

### Task: Set Up EF Core DbContext
- [ ] Create UsersDbContext with DbSets for users, oauth_providers, user_analytics
- [ ] Configure entity mappings (enums as int, UUID primary keys)
- [ ] Add connection string configuration
- [ ] Set up migrations folder
- [ ] Create initial migration

**Priority:** P0 (Critical)  
**Story Points:** 3

---

### Task: Implement Repository Pattern
- [ ] Create IUserRepository interface
- [ ] Implement UserRepository with CRUD operations
- [ ] Add unit of work pattern if needed
- [ ] Write repository unit tests

**Priority:** P0 (Critical)  
**Story Points:** 3

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
