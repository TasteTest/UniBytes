-- Orders service schema
-- Order lifecycle and line items. Status values stored as integers for EF compatibility:
-- order_status: 0=Pending, 1=Confirmed, 2=Preparing, 3=Ready, 4=Completed, 5=Cancelled, 6=Failed

CREATE EXTENSION IF NOT EXISTS pgcrypto;

CREATE TABLE IF NOT EXISTS orders (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID, -- reference to users service (not enforced cross-db)
    external_user_ref VARCHAR(255), -- optional external ID from auth service
    total_amount NUMERIC(12,2) NOT NULL,
    currency CHAR(3) DEFAULT 'ron',
    payment_status INTEGER NOT NULL DEFAULT 0, -- 0=NotPaid,1=Paid,2=Refunded (enum int)
    order_status INTEGER NOT NULL DEFAULT 0,
    placed_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    cancel_requested_at TIMESTAMP WITH TIME ZONE,
    canceled_at TIMESTAMP WITH TIME ZONE,
    metadata JSONB DEFAULT '{}',
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS order_items (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id UUID NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    menu_item_id UUID,
    name VARCHAR(255) NOT NULL,
    unit_price NUMERIC(10,2) NOT NULL,
    quantity INTEGER NOT NULL DEFAULT 1,
    modifiers JSONB DEFAULT '[]',
    total_price NUMERIC(12,2) NOT NULL,
    is_reward BOOLEAN NOT NULL DEFAULT FALSE,
    reward_id VARCHAR(255),
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

-- Indexes for common queries
CREATE INDEX IF NOT EXISTS idx_orders_user_id ON orders(user_id);
CREATE INDEX IF NOT EXISTS idx_orders_order_status ON orders(order_status);
CREATE INDEX IF NOT EXISTS idx_orders_placed_at ON orders(placed_at);
CREATE INDEX IF NOT EXISTS idx_order_items_order_id ON order_items(order_id);
