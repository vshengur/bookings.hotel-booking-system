# PAY-001: Убрать hardcoded значения из Payment Service

## Статус: Accepted (условно, ожидает integration test run в полном окружении)

## Описание
Payment Service содержит критические hardcoded значения: сумма платежа 250 EUR, return URL "https://example.com/return", PSP API key "demo-key". Для работающего бронирования необходимо, чтобы сумма бралась из запроса (реальная стоимость бронирования), валюта — из конфигурации, а URL-ы — из environment variables.

## Acceptance Criteria
- [ ] AC-1: `POST /payment/intent` принимает параметры `bookingId`, `amount` (в минимальных единицах валюты, cents), `currency` в теле запроса
- [ ] AC-2: Amount и currency не hardcoded, а берутся из запроса
- [ ] AC-3: Return URL и Cancel URL берутся из конфигурации (appsettings.json / env vars)
- [ ] AC-4: PSP API key берётся из конфигурации (не "demo-key")
- [ ] AC-5: Валидация входных данных: amount > 0, currency — допустимый ISO 4217 код, bookingId — valid GUID
- [ ] AC-6: PaymentIntent сохраняется в БД с привязкой к bookingId
- [ ] AC-7: RabbitMQ credentials берутся из конфигурации (не hardcoded "guest"/"guest")
- [ ] AC-8: Покрыто unit-тестами

## Функциональные требования
- FR-1: Создать DTO `CreatePaymentIntentRequest { Guid BookingId, long Amount, string Currency }`
- FR-2: Controller принимает DTO, валидирует, передаёт в сервис
- FR-3: Все конфигурационные значения — через IOptions<T> pattern
- FR-4: PaymentIntent entity хранит: BookingId, Amount, Currency, Status, ProviderRef, CreatedAt
- FR-5: MassTransit event после создания intent должен содержать typed payload (не anonymous object)

## Нефункциональные требования
- NFR-1: Время ответа API < 500ms (p95) — включает вызов к PSP
- NFR-2: Sensitive данные (API key) НЕ логируются

## Edge Cases / Corner Cases
- EC-1: Amount = 0 или отрицательный → 400 Bad Request
- EC-2: Неизвестная валюта → 400 Bad Request
- EC-3: BookingId, для которого уже есть PaymentIntent → вернуть существующий (idempotency) или 409 Conflict
- EC-4: PSP недоступен → 503 с retry suggestion

## Out of Scope
- Расчёт суммы бронирования (это делает booking-service через pricing-service)
- Поддержка нескольких PSP (только Stripe)

## Dependencies
- Booking Service должен передавать корректную сумму при создании PaymentIntent

## Priority: P0

## Assignee: Кодер

## Связь с Roadmap
- IMPLEMENTATION_ROADMAP.md: секция 4.1, задачи "Убрать hardcoded amount", "Получать actual booking amount из запроса", "Сохранять booking details в PaymentIntent"

## Ключевые файлы
- `services/payment-service/src/PaymentService.API/Controllers/PaymentController.cs` (hardcoded amount строка 33)
- `services/payment-service/src/PaymentService.Infrastructure/DependencyInjection.cs` (hardcoded RabbitMQ creds)
- `services/payment-service/src/PaymentService.Infrastructure/PSPClient/PaymentServiceProviderClient.cs` (demo-key)
- `services/payment-service/src/PaymentService.API/appsettings.json`
