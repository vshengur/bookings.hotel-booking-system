-- Create room_availabilities table
CREATE TABLE IF NOT EXISTS room_availabilities (
    id BIGSERIAL PRIMARY KEY,
    room_id BIGINT NOT NULL,
    check_in_date TIMESTAMP NOT NULL,
    check_out_date TIMESTAMP NOT NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'available',
    booking_id BIGINT,
    reserved_until TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_room_availabilities_room FOREIGN KEY (room_id) REFERENCES rooms(id) ON DELETE CASCADE
);

-- Create composite index on room_id and dates for availability checks
CREATE INDEX IF NOT EXISTS idx_room_availabilities_room_dates
    ON room_availabilities(room_id, check_in_date, check_out_date);

-- Create index on booking_id for booking lookups
CREATE INDEX IF NOT EXISTS idx_room_availabilities_booking_id ON room_availabilities(booking_id);

-- Create index on status for filtering
CREATE INDEX IF NOT EXISTS idx_room_availabilities_status ON room_availabilities(status);

-- Create index on reserved_until for cleanup queries
CREATE INDEX IF NOT EXISTS idx_room_availabilities_reserved_until ON room_availabilities(reserved_until);

-- Add constraint to ensure check_out_date is after check_in_date
ALTER TABLE room_availabilities
    ADD CONSTRAINT chk_room_availabilities_dates
    CHECK (check_out_date > check_in_date);
