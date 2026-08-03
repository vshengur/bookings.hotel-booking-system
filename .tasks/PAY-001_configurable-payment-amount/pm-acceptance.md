# PM Acceptance: PAY-001

## Решения по ревью #1 (2026-02-22)

### Принято к исправлению (блокируют merge):

| # | Уровень | Описание | Причина |
|---|---------|----------|---------|
| R-001 | Critical | Unique constraint на BookingId + обработка конфликтов | Двойной платёж — финансовый риск |
| R-002 | Critical | EF Core миграция (decimal→long + новые поля) | Без миграции сервис не запустится |
| R-004 | Major | CancelUrl → интегрировать в PaymentPayloadBuilder | AC-3 не полностью выполнен |
| R-005 | Major | ProviderRef → сохранять из ответа PSP | FR-4 не выполнен, нужен для refund |
| R-006 | Major | PSP ошибки → 503 вместо 500 | EC-4 из TASK.md |

### Отложено:

| # | Уровень | Описание | Куда отнесено |
|---|---------|----------|---------------|
| R-003 | Major | Breaking change API без версии | Допускается (MVP, consumers нет). Документировать в CHANGELOG. |
| R-007 | Major | Нет интеграционных тестов | Задача тестировщика при тестировании PAY-001 |
| R-008 | Minor | Бизнес-логика в контроллере | Техдолг, вынести в ARCH-001 или рефакторинг |

---

## Решения по ревью #2 (2026-02-22)

### Принято к исправлению:

| # | Уровень | Описание | Причина |
|---|---------|----------|---------|
| R-001 | Critical | Flow: PSP call до фиксации уникальности → двойной PSP intent при гонке | Финансовый риск. Паттерн: INSERT placeholder (Pending) → claim unique → PSP call → update. При конфликте — вернуть existing, PSP не вызывать. |
| R-009 | Major | PspResponseParser: GetString() на non-string бросает InvalidOperationException | Проверять ValueKind == String перед GetString(), добавить тесты |

### Уже принято в ревью #2 (не требует доработки):

| # | Статус |
|---|--------|
| R-002 | ✅ Accepted |
| R-004 | ✅ Accepted |
| R-006 | ✅ Accepted |

---

## Финальная приёмка PM (2026-02-22)

### Вердикт: ACCEPTED (условно)

### Проверка Acceptance Criteria:

| AC | Статус | Подтверждение |
|----|--------|--------------|
| AC-1 | ✅ | DTO `CreatePaymentIntentRequest { BookingId, Amount, Currency }`, подтверждено ревьюером и unit-тестами |
| AC-2 | ✅ | Amount/Currency из запроса, hardcoded значения удалены |
| AC-3 | ✅ | ReturnUrl + CancelUrl из `IOptions<PaymentSettings>`, R-004 исправлен |
| AC-4 | ✅ | API key из `IOptions<PspSettings>`, "demo-key" удалён |
| AC-5 | ✅ | FluentValidation: amount > 0, currency ISO 4217, bookingId valid GUID. 8 unit-тестов. |
| AC-6 | ✅ | PaymentIntent сохраняется с BookingId, unique constraint, placeholder-first flow |
| AC-7 | ✅ | RabbitMQ creds из `IOptions<RabbitMqSettings>`, "guest"/"guest" удалён |
| AC-8 | ✅ | 24 unit-теста + 5 integration-тестов (написаны, ожидают прогона) |

### Что прошло проверку:
- **Код-ревью:** APPROVED после 3 итераций (9 замечаний, все resolved)
- **Unit-тесты:** 24/24 passed
- **Integration-тесты:** 5 написаны (happy path, idempotency, validation, PSP failure, event)
- **Архитектура:** placeholder-first idempotency, defensive parsing, typed events

### Условия для production deployment:
1. `dotnet test Integration.Tests` — должны пройти в CI/CD
2. Миграция `20260222000000_UpdatePaymentIntentSchema` — прогнать up/down на staging PostgreSQL
3. NFR-1: p95 < 500ms — нагрузочный тест
4. NFR-2: аудит логов на отсутствие API key

### Технический долг (вынесен):
- **PAY-002**: Webhook HMAC validation
- **PAY-003**: Status string → enum (PaymentIntentStatus)
- **ARCH-001/R-008**: Бизнес-логика из контроллера в Application Service
- **R-003**: API versioning (при появлении внешних consumers)
