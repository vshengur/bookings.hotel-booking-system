ALTER TABLE room_availabilities
ADD COLUMN IF NOT EXISTS booking_reference VARCHAR(64);

CREATE INDEX IF NOT EXISTS idx_room_availabilities_booking_reference
ON room_availabilities (booking_reference);
