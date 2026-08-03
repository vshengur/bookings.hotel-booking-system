# BOOK-006: Интеграция расчёта стоимости через Pricing Service

## Статус: Open

## Описание
Booking Service должен получать актуальные цены из Pricing Service при создании бронирования, а не использовать hardcoded значения. Pricing Service уже реализован и предоставляет REST API и gRPC для расчёта цен с учётом стратегий (base, seasonal, weekend, promotional, dynamic).

## Acceptance Criteria
- [ ] AC-1: При создании бронирования цена запрашивается из Pricing Service через gRPC
- [ ] AC-2: Расчёт учитывает: roomId, checkIn, checkOut, количество гостей
- [ ] AC-3: Pricing breakdown сохраняется в бронировании: subtotal, taxes, fees, discounts, total
- [ ] AC-4: При недоступности pricing-service — fallback на базовую цену номера (из room-service)
- [ ] AC-5: gRPC клиент с retry и timeout

## Функциональные требования
- FR-1: gRPC клиент для pricing-service (CalculatePrice RPC)
- FR-2: PriceBreakdown value object в domain model
- FR-3: Сохранение breakdown в BookingLineItem

## Edge Cases / Corner Cases
- EC-1: Pricing-service недоступен → использовать base price из room-service
- EC-2: Цена изменилась между показом и бронированием → показать новую цену, запросить подтверждение
- EC-3: Dynamic pricing дал цену 0 → minimum price guard

## Priority: P2

## Assignee: Кодер

## Связь с Roadmap
- IMPLEMENTATION_ROADMAP.md: секция 3.5 "Расчёт стоимости"
