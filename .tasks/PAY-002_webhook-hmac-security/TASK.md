# PAY-002: Реализовать HMAC signature validation для Stripe webhook

## Статус: Open

## Описание
Webhook endpoint `POST /payment/webhook` не проверяет подпись от Stripe. Любой может отправить поддельный POST-запрос и подтвердить оплату без реального платежа. Это **критическая финансовая уязвимость**. Необходимо реализовать HMAC signature validation, idempotency и proper error handling.

## Acceptance Criteria
- [ ] AC-1: Webhook проверяет заголовок `Stripe-Signature` с помощью Stripe SDK `EventUtility.ConstructEvent()`
- [ ] AC-2: Webhook secret берётся из конфигурации (env var `STRIPE_WEBHOOK_SECRET`)
- [ ] AC-3: Запрос с невалидной или отсутствующей подписью — 401 Unauthorized
- [ ] AC-4: Idempotency: повторная обработка одного и того же webhook event (по eventId) — 200 OK без повторной публикации MassTransit-сообщений
- [ ] AC-5: Все webhook events логируются в отдельную таблицу WebhookLog (eventId, type, payload, receivedAt, processed)
- [ ] AC-6: PaymentIntent ищется безопасно (FirstOrDefault вместо First), при отсутствии — 404 с логированием ошибки
- [ ] AC-7: Null-safe обработка payload: проверка на null для всех полей перед использованием
- [ ] AC-8: Публикуемые MassTransit events используют typed contracts (PaymentAuthorized, PaymentFailed), а не anonymous objects
- [ ] AC-9: Покрыто unit-тестами: valid signature, invalid signature, duplicate event, missing intent

## Функциональные требования
- FR-1: Прочитать raw body запроса, передать в `EventUtility.ConstructEvent()` вместе с Stripe-Signature и webhookSecret
- FR-2: Создать таблицу WebhookLog: Id, EventId (unique), EventType, Payload (jsonb), ReceivedAtUtc, ProcessedAtUtc, Status (Received, Processed, Failed)
- FR-3: Перед обработкой проверить: есть ли EventId в WebhookLog — если да, вернуть 200 OK (idempotent)
- FR-4: Типизированные events: `PaymentAuthorized { BookingId, PaymentProviderRef }`, `PaymentFailed { BookingId, Error }`
- FR-5: Try-catch для всей обработки с логированием ошибок

## Нефункциональные требования
- NFR-1: Webhook обработка < 1s (Stripe ждёт ответ до 20s, но лучше быстрее)
- NFR-2: Rate limiting: максимум 100 webhook запросов в секунду
- NFR-3: Sensitive данные (card details) НЕ логируются

## Edge Cases / Corner Cases
- EC-1: Stripe отправляет webhook повторно (retry) — idempotency через EventId
- EC-2: Webhook приходит для несуществующего PaymentIntent — залогировать, вернуть 200 (не 404, чтобы Stripe не ретраил)
- EC-3: Webhook приходит раньше, чем PaymentIntent сохранён в БД — eventual consistency issue
- EC-4: Webhook с неизвестным event type — залогировать, вернуть 200
- EC-5: Stripe-Signature header отсутствует → 401
- EC-6: Body = empty → 400

## Out of Scope
- Обработка других Stripe event types (dispute, refund.created и т.д.) — только payment_intent.succeeded и payment_intent.payment_failed
- Dashboard для просмотра webhook логов

## Dependencies
- Stripe SDK (Stripe.net NuGet package)
- Stripe Webhook Secret (нужно получить из Stripe Dashboard)

## Priority: P0

## Assignee: Кодер

## Связь с Roadmap
- IMPLEMENTATION_ROADMAP.md: секция 4.2, задачи "HMAC signature validation", "idempotency для webhook calls", "Логирование webhook events"

## Ключевые файлы
- `services/payment-service/src/PaymentService.API/Controllers/PaymentController.cs` (строка 55: TODO validate HMAC)
- `services/payment-service/src/PaymentService.Infrastructure/Persistence/PaymentDbContext.cs` (добавить WebhookLog entity)
