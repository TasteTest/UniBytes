# CampusEats — Overview

CampusEats is a modular cafeteria ordering platform designed for universities, corporate campuses, and institutional cafeterias. It provides a clean, extensible backend built on .NET 8 Minimal APIs using Vertical Slice architecture. Each feature lives in its own slice (Menu, Orders, Kitchen, Payments, Loyalty), which makes the system maintainable and easy to scale across microservices.

## Core mission

- Deliver a reliable, fast, and extensible cafeteria ordering backend that supports multiple frontends and integration points (web, mobile, kiosk).
- Keep services small and focused so teams can iterate independently and deploy microservices safely.
- Provide clear, testable contracts (CQRS + MediatR) so business rules remain testable and understandable.

## Key values

- Simplicity: Make common tasks (placing an order, updating a menu item) frictionless for devs and customers.
- Observability: Instrument services so operations teams and developers can quickly diagnose and fix issues.
- Security & Privacy: Use proven patterns for auth (OAuth / NextAuth), protect payment data (Stripe best practices), and store only necessary user data.
- Resilience: Design for idempotency and safe retries across service boundaries (webhooks, payments).
- Extensibility: Model the domain to allow future features like promotions, advanced inventory forecasting, or multi-campus support.git a