# PAY-004: Улучшить обработку refund в Payment Service

## Статус: Open

## Описание
Текущий refund endpoint не принимает сумму возврата (всегда полный refund), не поддерживает partial refunds и не учитывает cancellation policy. Для production необходимо добавить параметр amount, поддержку частичного возврата и асинхронную обработку.

## Acceptance Criteria
- [ ] AC-1: `POST /payment/refund/{bookingId}` принимает опциональный параметр amount (если не указан — полный refund)
- [ ] AC-2: Partial refund: amount < original amount → частичный возврат
- [ ] AC-3: Нельзя вернуть больше, чем было оплачено → 400
- [ ] AC-4: Нельзя вернуть по уже полностью возвращённому платежу → 409
- [ ] AC-5: Refund статус сохраняется в БД
- [ ] AC-6: MassTransit event PaymentRefunded публикуется

## Edge Cases / Corner Cases
- EC-1: Двойной refund одновременно → только один должен пройти
- EC-2: Stripe refund failed → retry, пометить Failed
- EC-3: Partial refund + ещё один partial → сумма не превышает total

## Priority: P2

## Assignee: Кодер

## Связь с Roadmap
- IMPLEMENTATION_ROADMAP.md: секция 4.4 "Refund Processing"
