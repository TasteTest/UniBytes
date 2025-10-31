# DB Schemas and Conventions


This folder holds SQL schema files for each backend microservice. Conventions used across schemas:

- UUID primary keys (gen_random_uuid()) for service-level uniqueness and easy cross-service tracing.
- Timestamptz (TIMESTAMP WITH TIME ZONE) for created_at/updated_at fields.
- Enum values stored as integers to maintain compatibility with EF Core enums.
- Indexes for common query patterns (created_at, foreign keys, status fields).
- Soft deletes are not implemented by default — use `is_active` flags where appropriate.
- Where external providers are involved (OAuth, Stripe), we keep provider IDs and tokens but recommend storing secrets/encrypted fields in a secure secrets manager for production.

Files in this folder (simplified schemas):
- `users_service.sql` — user profiles, oauth_providers, user_analytics (simplified; removed newsletters, reports, bookmarks, premium_subscriptions)
- `menu_service.sql` — menu categories and menu_items (images/allergens/modifiers removed)
- `orders_service.sql` — orders and order_items (events removed)
- `kitchen_service.sql` — kitchen queue/tickets (inventory removed)
- `payments_service.sql` — payments and idempotency_keys (webhook-events/stripe customer mapping removed)
- `loyalty_service.sql` — loyalty accounts, transactions, redemptions

If you want, I can also add migration-ready SQL (separate up/down files) or EF Core model classes and DbContext scaffolding.
