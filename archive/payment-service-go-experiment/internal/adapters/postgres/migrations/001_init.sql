CREATE TABLE IF NOT EXISTS price_list(
    room_id TEXT NOT NULL,
    day DATE NOT NULL,
    amount_minor BIGINT NOT NULL,
    currency TEXT NOT NULL,
    PRIMARY KEY (room_id, day)
);

CREATE TABLE IF NOT EXISTS payment_intent(
    id TEXT PRIMARY KEY,
    booking_id TEXT NOT NULL,
    room_id TEXT NOT NULL,
    amount_minor BIGINT NOT NULL,
    currency TEXT NOT NULL,
    customer_email TEXT NOT NULL,
    psp_ref TEXT,
    status TEXT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL
);
