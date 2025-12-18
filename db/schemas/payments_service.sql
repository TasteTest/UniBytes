-- Payments service schema
-- Supports Stripe integration, test/mock payments, and subscription records.

CREATE EXTENSION IF NOT EXISTS pgcrypto;

CREATE TABLE IF NOT EXISTS payments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id UUID, -- reference to Orders service order id
    user_id UUID, -- reference to user service
    amount NUMERIC(12,2) NOT NULL,
    currency CHAR(3) DEFAULT 'ron',
    provider INTEGER NOT NULL DEFAULT 0, -- 0=Stripe, 1=Mock (enum int for EF)
    provider_payment_id VARCHAR(255), -- e.g., Stripe PaymentIntent ID
    provider_charge_id VARCHAR(255),
    status INTEGER NOT NULL DEFAULT 0, -- 0=Processing, 1=Succeeded, 2=Failed, 3=Refunded, 4=Cancelled (enum int for EF)
    raw_provider_response TEXT,
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
