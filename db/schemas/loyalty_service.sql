-- Loyalty service schema
-- Tracks loyalty balances, transactions, and redemptions.

CREATE EXTENSION IF NOT EXISTS pgcrypto;

CREATE TABLE IF NOT EXISTS loyalty_accounts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL UNIQUE,
    points_balance BIGINT NOT NULL DEFAULT 0,
    tier INTEGER NOT NULL DEFAULT 0, -- 0=Bronze,1=Silver,2=Gold (enum int)
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS loyalty_transactions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    loyalty_account_id UUID NOT NULL REFERENCES loyalty_accounts(id) ON DELETE CASCADE,
    change_amount BIGINT NOT NULL,
    reason VARCHAR(255), -- 'earn', 'redeem', 'adjust'
    reference_id UUID, -- optional link to order/payment
    metadata JSONB DEFAULT '{}',
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS loyalty_redemptions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    loyalty_account_id UUID NOT NULL REFERENCES loyalty_accounts(id) ON DELETE CASCADE,
    points_used BIGINT NOT NULL,
    reward_type VARCHAR(100), -- 'discount', 'free_item', etc.
    reward_metadata JSONB DEFAULT '{}',
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_loyalty_accounts_user_id ON loyalty_accounts(user_id);
CREATE INDEX IF NOT EXISTS idx_loyalty_transactions_loyalty_account_id ON loyalty_transactions(loyalty_account_id);
