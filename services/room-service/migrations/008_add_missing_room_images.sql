-- Add images for rooms that had none (202, 203, 302, 303, 304)
INSERT INTO room_images (room_id, url, title, description, is_primary, sort_order)
SELECT v.room_id, v.url, v.title, v.description, v.is_primary, v.sort_order
FROM (VALUES
  (5, 'https://images.unsplash.com/photo-1631049307264-da0ec9d70304', 'Double Room Main View',    'Elegant double room with queen bed',     true,  1),
  (5, 'https://images.unsplash.com/photo-1540518614846-7eded433c457', 'Bathroom',                 'En-suite marble bathroom',               false, 2),
  (6, 'https://images.unsplash.com/photo-1578683010236-d716f9a3f461', 'Deluxe Room Main View',    'Spacious deluxe room with balcony',       true,  1),
  (6, 'https://images.unsplash.com/photo-1584132967334-10e028bd69f7', 'Living Area',              'Comfortable seating area',               false, 2),
  (6, 'https://images.unsplash.com/photo-1560185007-5f0bb1866cab',    'Balcony View',             'Private balcony with view',              false, 3),
  (8, 'https://images.unsplash.com/photo-1631049552057-403cdb8f0658', 'Suite Main View',          'Premium suite with kitchenette',          true,  1),
  (8, 'https://images.unsplash.com/photo-1615874959474-d609969a20ed', 'Bedroom',                  'King size bedroom',                       false, 2),
  (8, 'https://images.unsplash.com/photo-1584132915807-fd1f5fbc078f', 'Mountain View',            'Stunning mountain view',                  false, 3),
  (9, 'https://images.unsplash.com/photo-1566665797739-1674de7a421a', 'Superior Double Main View','Superior double with king bed',           true,  1),
  (9, 'https://images.unsplash.com/photo-1582719508461-905c673771fd', 'Room Details',             'Premium amenities',                       false, 2),
  (10,'https://images.unsplash.com/photo-1611892440504-42a792e24d32', 'Business Single Main View','Stylish room for business travelers',     true,  1),
  (10,'https://images.unsplash.com/photo-1590490360182-c33d57733427', 'Work Desk',                'Ergonomic workspace',                     false, 2)
) AS v(room_id, url, title, description, is_primary, sort_order)
WHERE NOT EXISTS (
  SELECT 1 FROM room_images ri WHERE ri.room_id = v.room_id
);
