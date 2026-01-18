-- Create room_images table
CREATE TABLE IF NOT EXISTS room_images (
    id BIGSERIAL PRIMARY KEY,
    room_id BIGINT NOT NULL,
    url VARCHAR(1000) NOT NULL,
    title VARCHAR(200),
    description TEXT,
    is_primary BOOLEAN DEFAULT FALSE,
    sort_order INTEGER DEFAULT 0,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_room_images_room FOREIGN KEY (room_id) REFERENCES rooms(id) ON DELETE CASCADE
);

-- Create index on room_id for faster joins
CREATE INDEX IF NOT EXISTS idx_room_images_room_id ON room_images(room_id);

-- Create index on is_primary for finding primary images
CREATE INDEX IF NOT EXISTS idx_room_images_is_primary ON room_images(is_primary);

-- Create index on sort_order for ordering images
CREATE INDEX IF NOT EXISTS idx_room_images_sort_order ON room_images(sort_order);
