INSERT INTO price_list(room_id, day, amount_minor, currency) VALUES
('R1', CURRENT_DATE, 12000, 'EUR'),
('R1', CURRENT_DATE + INTERVAL '1 day', 12500, 'EUR'),
('R2', CURRENT_DATE, 9000, 'EUR')
ON CONFLICT DO NOTHING;
