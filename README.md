# CampusEats - Modular Cafeteria Ordering Platform

A production-ready cafeteria ordering platform built for campuses and institutions. The project combines a .NET 9 backend monolith organized by domain slices (menu, orders, payments, loyalty, users, kitchen, analytics, AI) with a modern Next.js frontend. It supports Google OAuth authentication, Stripe payments, loyalty points, real-time kitchen workflows, and AI-driven menu recommendations.

## Table of Contents

1. [Overview](#1-overview)
2. [System Architecture](#2-system-architecture)
3. [Key Features](#3-key-features)
4. [Technology Stack](#4-technology-stack)
5. [Project Structure](#5-project-structure)
6. [Services Overview](#6-services-overview)
   - [6.1 Backend Domains](#61-backend-domains)
   - [6.2 Frontend App](#62-frontend-app)
   - [6.3 API Summary](#63-api-summary)
7. [Quick Start](#7-quick-start)
   - [7.1 Run with Docker](#71-run-with-docker)
   - [7.2 Run Locally](#72-run-locally)
8. [Environment Configuration](#8-environment-configuration)
   - [8.1 Backend Variables](#81-backend-variables)
   - [8.2 Frontend Variables](#82-frontend-variables)
9. [Documentation](#9-documentation)
10. [Security Features](#10-security-features)
11. [Mobile Responsiveness](#11-mobile-responsiveness)
12. [AI Integration](#12-ai-integration)
13. [Performance and Scalability](#13-performance-and-scalability)
14. [Testing Strategy](#14-testing-strategy)
15. [Deployment](#15-deployment)
16. [Contributing](#16-contributing)
17. [License](#17-license)
18. [Why CampusEats](#18-why-campuseats)
19. [Support and Contact](#19-support-and-contact)
20. [Roadmap](#20-roadmap)

## 1. Overview

CampusEats is a modular cafeteria ordering platform designed for universities, corporate campuses, and institutional cafeterias. The backend follows a vertical-slice approach with clear domain boundaries and explicit service interfaces, while the frontend delivers a polished customer and staff experience for ordering, kitchen operations, and administration.

Core goals:

- Fast, reliable ordering across web, mobile, and kiosk-style frontends
- Clean separation of domains so teams can evolve features independently
- Secure authentication, payment handling, and role-based access control
- Extensible platform for future features like promotions and analytics

Primary user roles:

- User: browse menus, place orders, earn and redeem loyalty points
- Chef: manage order status and kitchen workflows
- Admin: manage users, roles, and menu content

Typical end-to-end flow:

1. User signs in with Google OAuth
2. User browses menu and creates an order
3. Checkout redirects to Stripe for payment
4. Webhook confirms payment and backend updates order state
5. Kitchen updates order status as it progresses

## 2. System Architecture

CampusEats uses a single backend service that unifies multiple domain slices under one API surface. This provides microservice-style modularity while keeping development and deployment streamlined.

Architecture highlights:

- Modular monolith with domain slices: users, menu, orders, payments, loyalty, kitchen, analytics, AI
- REST API with OpenAPI/Swagger in development
- PostgreSQL as the single source of truth for operational data
- Stripe Checkout for payment processing
- Google OAuth via NextAuth in the frontend with backend token validation
- Optional Azure Blob Storage for menu item images

Request lifecycle (simplified):

1. Frontend requests a token from Google OAuth via NextAuth
2. Backend validates JWT and enforces role checks
3. Domain services apply business rules and persist to PostgreSQL
4. External integrations (Stripe, Blob Storage, AI) run via service wrappers
5. Health checks expose service readiness at /health

## 3. Key Features

Ordering and Menu:

- Browse menu items by category
- Search and filter menu items
- Create and track orders with status changes
- Kitchen-focused order management flows
- Image upload and optional cloud storage for menu assets

Authentication and Users:

- Google OAuth login
- JWT-based API authentication
- Role management (User, Chef, Admin)
- OAuth provider linking for user identities

Payments:

- Stripe Checkout sessions
- Webhook verification for payment completion
- Payment records linked to orders
- Idempotent payment processing patterns

Loyalty:

- Loyalty accounts and points balances
- Points accrual and redemption
- Tiered loyalty support for future extensions

Analytics:

- User analytics events and session tracking
- Date-range reporting endpoints

AI:

- Menu recommendations based on preferences and constraints
- Extensible AI service layer with provider configuration

## 4. Technology Stack

Backend:

- .NET 9 (ASP.NET Core)
- Entity Framework Core 9 + Npgsql
- AutoMapper
- FluentValidation
- Stripe.net
- Azure.Storage.Blobs (optional image storage)
- OpenAPI/Swagger

Frontend:

- Next.js 14 (App Router)
- TypeScript
- Tailwind CSS + shadcn/ui
- NextAuth (Google OAuth)
- Zustand for cart state

Infrastructure:

- Docker + docker-compose
- PostgreSQL
- Health checks for container readiness

## 5. Project Structure

```
.
├── backend/                 # .NET backend monolith
├── frontend/                # Next.js frontend app
├── db/                      # SQL schemas and conventions
├── documentation/           # System and integration docs
├── backlogs/                # Feature backlogs per domain
├── diagrams/                # Architecture diagrams (PlantUML)
├── scripts/                 # Deployment and utility scripts
├── docker-compose.yml       # Local containers (db + backend)
└── README.md                # This file
```

## 6. Services Overview

### 6.1 Backend Domains

The backend exposes domain endpoints via controller groups. A single API host serves all routes.

- Auth and Users: Google OAuth login and user management
- Menu and Categories: menu item CRUD and image upload
- Orders: creation, user order history, and status updates
- Payments: Stripe checkout, webhooks, verification
- Loyalty: accounts, balance, add and redeem points
- Analytics: event tracking and session analytics
- AI: preference-based menu recommendations

Primary API route groups:

- /api/auth
- /api/users
- /api/oauthproviders
- /api/menuitems
- /api/categories
- /api/orders
- /api/payments
- /api/loyaltyaccounts
- /api/useranalytics
- /api/ai

Domain behavior notes:

- Orders: only Users can place orders; only Chefs can update order status
- Users: only Admins can change user roles
- Payments: webhooks require Stripe signature verification

### 6.3 API Summary

Common endpoints by area:

- Auth: POST /api/auth/google, GET /api/auth/me
- Menu: GET /api/menuitems, GET /api/categories
- Orders: POST /api/orders, GET /api/orders/user/{userId}
- Payments: POST /api/payments/checkout-session, GET /api/payments/verify/{sessionId}
- Loyalty: GET /api/loyaltyaccounts/user/{userId}, POST /api/loyaltyaccounts/add-points
- Analytics: GET /api/useranalytics/user/{userId}, POST /api/useranalytics
- AI: POST /api/ai/chat

### 6.2 Frontend App

The frontend provides customer, staff, and admin experiences with shared UI components and consistent theming.

Key UI areas:

- Customer flows: menu, cart, checkout, orders, loyalty
- Staff flows: kitchen dashboard and order management
- Admin flows: menu and user management

## 7. Quick Start

Prerequisites:

- Docker and Docker Compose
- .NET 9 SDK
- Node.js 18+ and npm

### 7.1 Run with Docker

```bash
docker compose up -d
```

This starts:

- PostgreSQL on port 5432
- Backend API on port 5267 (container port 8080)

If the environment is Development, Swagger UI is available at /swagger and health checks at /health.

### 7.2 Run Locally

```bash
cd backend
dotnet run
```

Frontend (local development):

```bash
cd frontend
npm install
npm run dev
```

For local backend configuration without Docker, add a backend/.env.local file with POSTGRES_* variables.

## 8. Environment Configuration

Backend configuration uses environment variables and optionally a local .env.local file loaded at startup.

### 8.1 Backend Variables

Minimum backend variables:

- POSTGRES_HOST, POSTGRES_PORT, POSTGRES_DB, POSTGRES_USER, POSTGRES_PASSWORD
- STRIPE_SECRET_KEY, STRIPE_WEBHOOK_SECRET (if using Stripe)

Optional backend variables:

- FRONTEND_URL for CORS and redirects
- AZURE_STORAGE_CONNECTION_STRING for menu item images
- OpenRouter:BaseUrl for AI provider routing

### 8.2 Frontend Variables

Frontend variables live in frontend/.env and are documented in the frontend README.

Templates and references:

- Root env template: [.env.example](.env.example)
- Frontend env template: [frontend/.env.example](frontend/.env.example)

## 9. Documentation

Detailed documentation is available in the documentation folder and service-specific READMEs.

- System overview: [documentation/OVERVIEW.md](documentation/OVERVIEW.md)
- Google OAuth setup: [documentation/Auth-schema.md](documentation/Auth-schema.md)
- Stripe integration: [documentation/STRIPE_INTEGRATION.md](documentation/STRIPE_INTEGRATION.md)
- Frontend guide: [frontend/README.md](frontend/README.md)
- Database schemas: [db/README.md](db/README.md)

## 10. Security Features

- Google OAuth login via NextAuth
- JWT authentication middleware for API access
- Role-based authorization (User, Chef, Admin)
- Stripe webhook signature verification
- CORS configuration for approved origins
- Environment-based configuration for secrets and keys

## 11. Mobile Responsiveness

The frontend is built with responsive layouts and accessible components. It targets mobile-first usage for students and staff and adapts to kiosk-style screens.

User experience highlights:

- Mobile-first layouts with clear touch targets
- Kitchen views optimized for large displays
- Consistent component system for accessibility

## 12. AI Integration

The AI module provides menu recommendations based on user preferences, dietary restrictions, and goals. The backend integrates with a configurable OpenRouter endpoint and exposes a single chat-style endpoint under /api/ai.

AI requests include:

- Dietary restrictions and allergies
- Cuisine preferences
- Calorie goals or meal size

## 13. Performance and Scalability

- Modular domain separation to keep services isolated
- Database indexing and schema conventions for common queries
- Image processing handled via Blob storage integration
- Redis is optional and can be enabled if needed (see env template)
- Docker health checks for faster orchestration recovery

## 14. Testing Strategy

- Backend tests are executed during Docker builds
- Unit and integration tests live under backend/Tests and Tests
- Frontend includes linting and build checks via npm scripts
- Stripe webhooks can be tested with the Stripe CLI

## 15. Deployment

Typical deployment flow:

- Build and publish backend container
- Provision PostgreSQL instance
- Configure environment variables for database, Stripe, and OAuth
- Deploy frontend with Next.js build output

Scripts for redeploying are available in the scripts folder.

## 16. Contributing

1. Fork the repository and create a feature branch
2. Follow the existing architecture patterns and code style
3. Add tests for new functionality
4. Update documentation as needed
5. Open a pull request with a clear description

## 17. License

MIT. See repository license for details.

## 18. Why CampusEats

- Purpose-built for campus dining operations
- Clean domain separation that scales with teams
- Secure payment and authentication flows
- Modern UI that feels fast and intuitive
- AI-driven personalization for smarter menus

## 19. Support and Contact

For issues or questions:

- Open a GitHub issue with reproduction steps
- Include logs and relevant configuration details

## 20. Roadmap

Planned enhancements:

- Real-time order updates via WebSockets
- Advanced promotions and discounts engine
- Inventory and availability forecasting
- Multi-campus support
- Push notifications and mobile app companion
- Expanded analytics dashboards and admin reporting
