-- Kitchen service schema
-- Tracks kitchen queue and inventory. Inventory can be used by Kitchen and Orders services.

CREATE EXTENSION IF NOT EXISTS pgcrypto;

CREATE TABLE IF NOT EXISTS kitchen_orders (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id UUID NOT NULL, -- cross-service reference to Orders.order.id
    station VARCHAR(100), -- e.g., 'Grill', 'Salad', 'Drinks'
    status INTEGER NOT NULL DEFAULT 0, -- 0=Queued,1=InProgress,2=Ready,3=Completed,4=Cancelled
    assigned_to VARCHAR(100), -- chef/kitchen user id or name
    started_at TIMESTAMPTZ,
    ready_at TIMESTAMPTZ,
    completed_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS kitchen_items (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    kitchen_order_id UUID NOT NULL REFERENCES kitchen_orders(id) ON DELETE CASCADE,
    order_item_id UUID, -- maps back to Orders.order_items.id
    name VARCHAR(255) NOT NULL,
    note TEXT,
    status INTEGER NOT NULL DEFAULT 0, -- 0=Queued,1=Preparing,2=Ready,3=Completed
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Inventory (basic)
-- Simplified kitchen schema: keep kitchen queue and items. Inventory tables removed to reduce complexity.

CREATE INDEX IF NOT EXISTS idx_kitchen_orders_order_id ON kitchen_orders(order_id);
CREATE INDEX IF NOT EXISTS idx_kitchen_orders_status ON kitchen_orders(status);
