-- Payments service schema
-- Supports Stripe integration, test/mock payments, and subscription records.

CREATE EXTENSION IF NOT EXISTS pgcrypto;

CREATE TABLE IF NOT EXISTS payments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id UUID, -- reference to Orders service order id
    user_id UUID, -- reference to user service
    amount NUMERIC(12,2) NOT NULL,
    currency CHAR(3) DEFAULT 'USD',
    provider VARCHAR(50) NOT NULL DEFAULT 'stripe', -- 'stripe' or 'mock'
    provider_payment_id VARCHAR(255), -- e.g., Stripe PaymentIntent ID
    provider_charge_id VARCHAR(255),
    status VARCHAR(50) NOT NULL DEFAULT 'processing', -- processing, succeeded, failed, refunded
    raw_provider_response JSONB,
    failure_message TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Idempotency keys for safe retry from clients
CREATE TABLE IF NOT EXISTS idempotency_keys (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    key VARCHAR(255) UNIQUE NOT NULL,
    user_id UUID,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    expires_at TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS idx_payments_order_id ON payments(order_id);
CREATE INDEX IF NOT EXISTS idx_payments_user_id ON payments(user_id);

-- Example: store ephemeral keys or encrypted secrets in a separate secrets manager, not in DB.
