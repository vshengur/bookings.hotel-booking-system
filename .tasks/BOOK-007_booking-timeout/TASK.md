# BOOK-007: Проверить и доработать booking timeout handling

## Статус: Open

## Описание
Saga уже планирует timeout через 15 минут при создании бронирования. Необходимо проверить, что timeout корректно работает end-to-end: номер освобождается, пользователь получает уведомление, статус меняется на Expired. Также нужно добавить expiresAt в API response, чтобы фронтенд мог показать countdown timer.

## Acceptance Criteria
- [ ] AC-1: Через 15 минут после создания бронирования без оплаты — статус меняется на Expired
- [ ] AC-2: При Expired — номер освобождается (Release через IInventoryGateway)
- [ ] AC-3: API response при создании бронирования содержит expiresAt (ISO 8601)
- [ ] AC-4: GET /api/booking/{id} содержит expiresAt
- [ ] AC-5: BookingExpired event публикуется для notification-service

## Edge Cases / Corner Cases
- EC-1: Оплата проходит за 1 секунду до timeout → payment должен выиграть (unschedule timeout)
- EC-2: Timeout firing при недоступном room-service → retry release

## Priority: P2

## Assignee: Кодер

## Связь с Roadmap
- IMPLEMENTATION_ROADMAP.md: секция 3.6 "Booking timeout handling"
