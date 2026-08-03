# BOOK-001: Реализовать confirm и cancel endpoints в Booking Service

## Статус: Open

## Описание
Сейчас endpoints `POST /api/booking/{id}/confirm` и `POST /api/booking/{id}/cancel` — заглушки: handler-ы закомментированы, возвращается пустой 204 NoContent. Необходимо реализовать полноценную логику подтверждения и отмены бронирований, чтобы пользователь мог отменить бронирование, а PMS-интеграция могла подтвердить его.

## Acceptance Criteria
- [ ] AC-1: `POST /api/booking/{id}/cancel` меняет статус бронирования на Cancelled, если текущий статус позволяет отмену (Created, AwaitingPayment, Reserved, Confirmed)
- [ ] AC-2: `POST /api/booking/{id}/cancel` публикует MassTransit-сообщение `CancelBooking` в Saga
- [ ] AC-3: `POST /api/booking/{id}/confirm` меняет статус бронирования на Confirmed (вызывается от PMS или вручную)
- [ ] AC-4: Отмена бронирования в статусе Reserved или Confirmed инициирует refund через Saga
- [ ] AC-5: Попытка отменить уже отменённое/failed/expired бронирование возвращает 409 Conflict
- [ ] AC-6: Попытка подтвердить бронирование не в статусе Reserved возвращает 409 Conflict
- [ ] AC-7: Несуществующий bookingId возвращает 404 Not Found
- [ ] AC-8: Оба endpoint-а покрыты unit-тестами

## Функциональные требования
- FR-1: CancelBookingCommandHandler — загружает бронирование из репозитория, проверяет допустимость перехода, вызывает booking.MarkCancelled(reason), сохраняет, публикует CancelBooking через MassTransit
- FR-2: ConfirmBookingCommandHandler — загружает бронирование, проверяет статус == Reserved, вызывает booking.MarkConfirmed(), сохраняет
- FR-3: Cancel должен принимать опциональный параметр reason (string) в body запроса
- FR-4: При отмене с оплатой — Saga должна обработать CancelBooking и инициировать RefundPaymentCommand

## Нефункциональные требования
- NFR-1: Время ответа API < 200ms (p95)
- NFR-2: Операции идемпотентны: повторный cancel уже cancelled бронирования — 409, не дублирует events

## Edge Cases / Corner Cases
- EC-1: Отмена бронирования в момент, когда Saga обрабатывает PaymentAuthorized (race condition)
- EC-2: Двойной вызов cancel одновременно (concurrency)
- EC-3: Отмена бронирования, которое уже Expired
- EC-4: Confirm бронирования, платёж за которое ещё не прошёл (статус AwaitingPayment)
- EC-5: Cancel после confirm (Confirmed → Cancelled) — допускается ли?

## Out of Scope
- Cancellation policy (штрафы за позднюю отмену) — отдельная задача
- Partial refund — отдельная задача (PAY-004)
- Email-уведомление об отмене — задача NOTIFY-001

## Dependencies
- Существующий BookingStateMachine (Saga) должен корректно обрабатывать CancelBooking из любого состояния
- Saga уже имеет transition DuringAny → Cancelled при получении CancelBooking

## Priority: P0

## Assignee: Кодер

## Связь с Roadmap
- IMPLEMENTATION_ROADMAP.md: секция 3.1, задачи "Реализовать POST /api/booking/{id}/confirm" и "POST /api/booking/{id}/cancel"

## Ключевые файлы
- `services/bookings-service/src/Api/Controllers/BookingController.cs` (строки confirm/cancel — заглушки)
- `services/bookings-service/src/Application/Commands/` (ConfirmBookingCommand, CancelBookingCommand — создать handlers)
- `services/bookings-service/src/Domain/Aggregates/Booking/Booking.cs` (MarkConfirmed, MarkCancelled — уже есть)
- `services/bookings-service/src/Infrastructure/Messaging/MassTransit/BookingStateMachine.cs`
