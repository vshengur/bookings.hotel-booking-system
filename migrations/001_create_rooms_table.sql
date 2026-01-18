-- Create rooms table
CREATE TABLE IF NOT EXISTS rooms (
    id BIGSERIAL PRIMARY KEY,
    room_number VARCHAR(50) NOT NULL UNIQUE,
    room_type VARCHAR(100) NOT NULL,
    floor INTEGER NOT NULL,
    capacity INTEGER NOT NULL,
    bed_type VARCHAR(100),
    size DECIMAL(10, 2),
    description TEXT,
    base_price DECIMAL(10, 2) NOT NULL,
    currency VARCHAR(3) DEFAULT 'EUR',
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMP
);

-- Create index on room_number for faster lookups
CREATE INDEX IF NOT EXISTS idx_rooms_room_number ON rooms(room_number);

-- Create index on room_type for filtering
CREATE INDEX IF NOT EXISTS idx_rooms_room_type ON rooms(room_type);

-- Create index on deleted_at for soft deletes
CREATE INDEX IF NOT EXISTS idx_rooms_deleted_at ON rooms(deleted_at);

-- Create index on is_active for active room queries
CREATE INDEX IF NOT EXISTS idx_rooms_is_active ON rooms(is_active);
