-- Create room_amenities table
CREATE TABLE IF NOT EXISTS room_amenities (
    id BIGSERIAL PRIMARY KEY,
    room_id BIGINT NOT NULL,
    amenity_type VARCHAR(100) NOT NULL,
    name VARCHAR(200) NOT NULL,
    description TEXT,
    icon_url VARCHAR(500),
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_room_amenities_room FOREIGN KEY (room_id) REFERENCES rooms(id) ON DELETE CASCADE
);

-- Create index on room_id for faster joins
CREATE INDEX IF NOT EXISTS idx_room_amenities_room_id ON room_amenities(room_id);

-- Create index on amenity name for searching
CREATE INDEX IF NOT EXISTS idx_room_amenities_name ON room_amenities(name);

-- Create index on amenity type for filtering
CREATE INDEX IF NOT EXISTS idx_room_amenities_type ON room_amenities(amenity_type);
