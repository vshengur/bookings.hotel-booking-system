# PAY-003: Типизировать MassTransit контракты в Payment Service

## Статус: Open

## Описание
Payment Service публикует MassTransit-сообщения как anonymous objects (`new { BookingId = ..., Status = "Succeeded" }`). Booking Service Saga ожидает типизированные контракты (`PaymentAuthorized`, `PaymentFailed`). Из-за этого несоответствия Saga не получает events от payment-service. Необходимо использовать общие типизированные контракты из `common/Booking.Sharing/`.

## Acceptance Criteria
- [ ] AC-1: Payment Service использует typed contracts `PaymentAuthorized`, `PaymentFailed` из общей сборки `Booking.Contracts`
- [ ] AC-2: При успешном webhook (payment_intent.succeeded) публикуется `PaymentAuthorized { BookingId, PaymentProviderRef }`
- [ ] AC-3: При неуспешном webhook (payment_intent.payment_failed) публикуется `PaymentFailed { BookingId, Error }`
- [ ] AC-4: При создании intent публикуется типизированный event (не anonymous object)
- [ ] AC-5: При refund публикуется типизированный event
- [ ] AC-6: Booking Service Saga корректно обрабатывает events от payment-service (end-to-end проверка)
- [ ] AC-7: Покрыто integration-тестом с MassTransit InMemoryTestHarness

## Функциональные требования
- FR-1: Добавить reference на `Booking.Contracts` в Payment Service
- FR-2: Заменить все `bus.Publish(new { ... })` на `bus.Publish<PaymentAuthorized>(...)` и `bus.Publish<PaymentFailed>(...)`
- FR-3: Убедиться, что MassTransit message namespaces совпадают между producer и consumer

## Нефункциональные требования
- NFR-1: Не ломать существующие consumers — backward compatible

## Edge Cases / Corner Cases
- EC-1: Namespace mismatch между Booking.Contracts и payment-service → MassTransit не десериализует
- EC-2: Старые сообщения в RabbitMQ (anonymous type) — могут остаться в очереди

## Out of Scope
- Создание новых event types
- Версионирование контрактов

## Dependencies
- `common/Booking.Sharing/Booking.Contracts/Messages.cs` — контракты уже определены
- PAY-002 (webhook) — логически связана, можно делать вместе

## Priority: P1

## Assignee: Кодер

## Связь с Roadmap
- IMPLEMENTATION_ROADMAP.md: неявно связана с секциями 3 и 4 — корректная интеграция через messaging
