-- Seed data for testing
-- Note: Pricing information has been moved to Pricing Service

-- Insert sample rooms
INSERT INTO rooms (room_number, room_type, floor, capacity, bed_type, size, description, base_price, currency, is_active)
VALUES
    ('101', 'single', 1, 1, 'Single Bed', 20.00, 'Cozy single room with city view. Perfect for solo travelers.', 80.00, 'EUR', true),
    ('102', 'double', 1, 2, 'Double Bed', 25.00, 'Comfortable double room with modern amenities and garden view.', 120.00, 'EUR', true),
    ('103', 'twin', 1, 2, 'Twin Beds', 25.00, 'Spacious room with two separate beds, ideal for friends or colleagues.', 110.00, 'EUR', true),
    ('201', 'suite', 2, 3, 'King Bed + Sofa Bed', 45.00, 'Luxurious suite with separate living area, king bed, and stunning city panorama.', 250.00, 'EUR', true),
    ('202', 'double', 2, 2, 'Queen Bed', 28.00, 'Elegant double room with queen bed and marble bathroom.', 140.00, 'EUR', true),
    ('203', 'deluxe', 2, 3, 'King Bed + Twin Bed', 40.00, 'Deluxe family room with king bed, twin bed, and balcony.', 200.00, 'EUR', true),
    ('301', 'penthouse', 3, 4, '2 King Beds', 80.00, 'Exclusive penthouse with panoramic terrace, two bedrooms, and luxury finishes.', 600.00, 'EUR', true),
    ('302', 'suite', 3, 3, 'King Bed + Sofa Bed', 50.00, 'Premium suite with kitchenette, workspace, and mountain view.', 300.00, 'EUR', true),
    ('303', 'double', 3, 2, 'King Bed', 30.00, 'Superior double room with king bed and premium amenities.', 160.00, 'EUR', true),
    ('304', 'single', 3, 1, 'Queen Bed', 22.00, 'Stylish single room with queen bed, perfect for business travelers.', 90.00, 'EUR', true);

-- Insert amenities for Room 101 (Single)
INSERT INTO room_amenities (room_id, amenity_type, name, description, icon_url)
VALUES
    (1, 'basic', 'WiFi', 'Free high-speed wireless internet', 'https://example.com/icons/wifi.svg'),
    (1, 'basic', 'Air Conditioning', 'Climate control system', 'https://example.com/icons/ac.svg'),
    (1, 'entertainment', 'TV', '32" Smart TV with streaming', 'https://example.com/icons/tv.svg'),
    (1, 'bathroom', 'Private Bathroom', 'En-suite bathroom with shower', 'https://example.com/icons/bathroom.svg');

-- Insert amenities for Room 102 (Double)
INSERT INTO room_amenities (room_id, amenity_type, name, description, icon_url)
VALUES
    (2, 'basic', 'WiFi', 'Free high-speed wireless internet', 'https://example.com/icons/wifi.svg'),
    (2, 'basic', 'Air Conditioning', 'Climate control system', 'https://example.com/icons/ac.svg'),
    (2, 'entertainment', 'TV', '42" Smart TV with streaming', 'https://example.com/icons/tv.svg'),
    (2, 'bathroom', 'Private Bathroom', 'En-suite bathroom with bathtub', 'https://example.com/icons/bathroom.svg'),
    (2, 'basic', 'Mini Bar', 'Stocked mini refrigerator', 'https://example.com/icons/minibar.svg'),
    (2, 'view', 'Garden View', 'Beautiful garden view', 'https://example.com/icons/view.svg');

-- Insert amenities for Room 103 (Twin)
INSERT INTO room_amenities (room_id, amenity_type, name, description, icon_url)
VALUES
    (3, 'basic', 'WiFi', 'Free high-speed wireless internet', 'https://example.com/icons/wifi.svg'),
    (3, 'basic', 'Air Conditioning', 'Climate control system', 'https://example.com/icons/ac.svg'),
    (3, 'entertainment', 'TV', '42" Smart TV', 'https://example.com/icons/tv.svg'),
    (3, 'bathroom', 'Private Bathroom', 'En-suite bathroom', 'https://example.com/icons/bathroom.svg'),
    (3, 'basic', 'Work Desk', 'Spacious work area', 'https://example.com/icons/desk.svg');

-- Insert amenities for Room 201 (Suite)
INSERT INTO room_amenities (room_id, amenity_type, name, description, icon_url)
VALUES
    (4, 'basic', 'WiFi', 'Free high-speed wireless internet', 'https://example.com/icons/wifi.svg'),
    (4, 'basic', 'Air Conditioning', 'Premium climate control', 'https://example.com/icons/ac.svg'),
    (4, 'entertainment', 'TV', '55" Smart TV in bedroom + 42" in living room', 'https://example.com/icons/tv.svg'),
    (4, 'bathroom', 'Luxury Bathroom', 'Marble bathroom with jacuzzi', 'https://example.com/icons/bathroom.svg'),
    (4, 'basic', 'Mini Bar', 'Premium stocked mini bar', 'https://example.com/icons/minibar.svg'),
    (4, 'kitchen', 'Kitchenette', 'Small kitchen with coffee maker', 'https://example.com/icons/kitchen.svg'),
    (4, 'view', 'City View', 'Panoramic city view', 'https://example.com/icons/view.svg'),
    (4, 'basic', 'Safe', 'In-room safe', 'https://example.com/icons/safe.svg');

-- Insert amenities for Room 301 (Penthouse)
INSERT INTO room_amenities (room_id, amenity_type, name, description, icon_url)
VALUES
    (7, 'basic', 'WiFi', 'Free ultra high-speed wireless internet', 'https://example.com/icons/wifi.svg'),
    (7, 'basic', 'Air Conditioning', 'Premium multi-zone climate control', 'https://example.com/icons/ac.svg'),
    (7, 'entertainment', 'TV', '65" Smart TV in master + 55" in second bedroom', 'https://example.com/icons/tv.svg'),
    (7, 'bathroom', 'Luxury Bathrooms', '2 marble bathrooms with rain showers', 'https://example.com/icons/bathroom.svg'),
    (7, 'basic', 'Mini Bar', 'Premium stocked mini bar', 'https://example.com/icons/minibar.svg'),
    (7, 'kitchen', 'Full Kitchen', 'Fully equipped kitchen', 'https://example.com/icons/kitchen.svg'),
    (7, 'view', 'Terrace', 'Private panoramic terrace', 'https://example.com/icons/terrace.svg'),
    (7, 'basic', 'Safe', 'In-room safe', 'https://example.com/icons/safe.svg'),
    (7, 'entertainment', 'Sound System', 'Premium sound system', 'https://example.com/icons/sound.svg'),
    (7, 'basic', 'Wine Cooler', 'Built-in wine cooler', 'https://example.com/icons/wine.svg');

-- Insert room images
-- Room 101
INSERT INTO room_images (room_id, url, title, description, is_primary, sort_order)
VALUES
    (1, 'https://images.unsplash.com/photo-1611892440504-42a792e24d32', 'Single Room Main View', 'Main view of the single room', true, 1),
    (1, 'https://images.unsplash.com/photo-1590490360182-c33d57733427', 'Bathroom', 'Modern bathroom', false, 2);

-- Room 102
INSERT INTO room_images (room_id, url, title, description, is_primary, sort_order)
VALUES
    (2, 'https://images.unsplash.com/photo-1566665797739-1674de7a421a', 'Double Room Main View', 'Comfortable double room', true, 1),
    (2, 'https://images.unsplash.com/photo-1582719478250-c89cae4dc85b', 'Garden View', 'View from the window', false, 2);

-- Room 103
INSERT INTO room_images (room_id, url, title, description, is_primary, sort_order)
VALUES
    (3, 'https://images.unsplash.com/photo-1598928506311-c55ded91a20c', 'Twin Room Main View', 'Spacious twin room', true, 1),
    (3, 'https://images.unsplash.com/photo-1595526114035-0d45ed16cfbf', 'Work Area', 'Comfortable workspace', false, 2);

-- Room 201 (Suite)
INSERT INTO room_images (room_id, url, title, description, is_primary, sort_order)
VALUES
    (4, 'https://images.unsplash.com/photo-1631049307264-da0ec9d70304', 'Suite Main View', 'Luxurious suite', true, 1),
    (4, 'https://images.unsplash.com/photo-1584132967334-10e028bd69f7', 'Living Area', 'Spacious living area', false, 2),
    (4, 'https://images.unsplash.com/photo-1615874959474-d609969a20ed', 'Bedroom', 'King bedroom', false, 3);

-- Room 301 (Penthouse)
INSERT INTO room_images (room_id, url, title, description, is_primary, sort_order)
VALUES
    (7, 'https://images.unsplash.com/photo-1582719508461-905c673771fd', 'Penthouse Main View', 'Exclusive penthouse', true, 1),
    (7, 'https://images.unsplash.com/photo-1560185893-a55cbc8c57e8', 'Terrace', 'Panoramic terrace view', false, 2),
    (7, 'https://images.unsplash.com/photo-1578683010236-d716f9a3f461', 'Master Bedroom', 'Luxury master bedroom', false, 3),
    (7, 'https://images.unsplash.com/photo-1556020685-ae41abfc9365', 'Kitchen', 'Fully equipped kitchen', false, 4);
