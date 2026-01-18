-- Remove pricing fields from rooms table (moved to Pricing Service)
ALTER TABLE rooms DROP COLUMN IF EXISTS base_price;
ALTER TABLE rooms DROP COLUMN IF EXISTS currency;
