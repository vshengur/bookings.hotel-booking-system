# Тест-план: PAY-001 configurable payment amount

## Scope
- Проверка `POST /payment/intent` после удаления hardcoded значений в `payment-service`.
- Проверка конфигурации (`Payment`, `Psp`, `RabbitMq`) и критичных сценариев idempotency/PSP failure.
- Проверка регрессий из ревью `R-001..R-009` (в рамках PAY-001).

## Out of Scope
- Webhook HMAC (PAY-002).
- Multi-PSP сценарии.
- Полный end-to-end флоу booking-service -> payment-service -> frontend.

## Подход и уровни тестирования
- **Unit:** существующие тесты валидаторов, payload builder, parser (24 шт.).
- **Integration/Component:** API через `WebApplicationFactory`, test DB (SQLite in-memory), публикация события через in-memory publish harness (spy/mock publish endpoint).
- **Manual/Env checks:** конфигурация через env vars, миграции и latency в полном окружении.

## Матрица покрытия требований
- **AC-1:** TC-INT-001, TC-INT-003
- **AC-2:** TC-INT-001, TC-UNIT-001..008
- **AC-3:** TC-UNIT-009..012, TC-CONF-001
- **AC-4:** TC-CONF-002
- **AC-5:** TC-UNIT-001..008, TC-INT-003
- **AC-6:** TC-INT-001, TC-INT-004
- **AC-7:** TC-CONF-003
- **AC-8:** TC-UNIT-ALL, TC-INT-ALL
- **FR-1:** TC-INT-001, TC-INT-003
- **FR-2:** TC-INT-001, TC-INT-003
- **FR-3:** TC-CONF-001..003
- **FR-4:** TC-INT-001, TC-INT-004, TC-UNIT-013..024
- **FR-5:** TC-INT-001, TC-INT-005
- **NFR-1:** TC-NFR-001
- **NFR-2:** TC-NFR-002
- **EC-1:** TC-INT-003
- **EC-2:** TC-INT-003
- **EC-3:** TC-INT-002
- **EC-4:** TC-INT-004
- **R-001:** TC-INT-002
- **R-002:** TC-CONF-004
- **R-004:** TC-UNIT-009..012
- **R-005:** TC-UNIT-013..024, TC-INT-001
- **R-006:** TC-INT-004
- **R-009:** TC-UNIT-020..024

## Детализация тест-кейсов

### TC-UNIT-ALL: Прогон unit-тестов PAY-001
- **Тип:** Unit
- **Приоритет:** P0
- **Preconditions:** Собирается `services/payment-service/tests/Domain.Tests`.
- **Steps:** `dotnet test services/payment-service/tests/Domain.Tests/Domain.Tests.csproj`
- **Expected:** 24/24 passed:
  - 8 validator
  - 4 builder
  - 6 parser базовых
  - 5 parser edge cases (R-009)
  - 1 вспомогательный

### TC-INT-001: Happy path create intent
- **Тип:** Integration
- **Приоритет:** P0
- **Preconditions:** test host поднят через `WebApplicationFactory`.
- **Steps:**
  1. `POST /payment/intent` с валидным `bookingId`, `amount`, `currency`.
  2. Проверить API response и запись в DB.
  3. Проверить факт публикации `PaymentIntentCreated`.
- **Expected:** HTTP 202, status `created`, `ProviderRef` сохранён, событие опубликовано 1 раз.

### TC-INT-002: Idempotency (R-001, EC-3)
- **Тип:** Integration
- **Приоритет:** P0
- **Steps:**
  1. Дважды отправить одинаковый request с тем же `BookingId`.
  2. Сравнить `IntentId` в ответах.
  3. Проверить количество вызовов PSP.
- **Expected:** первый ответ 202, второй 200 с тем же `IntentId`, PSP вызван ровно один раз.

### TC-INT-003: Validation errors (AC-5, EC-1, EC-2)
- **Тип:** Integration
- **Приоритет:** P0
- **Steps:**
  1. Отправить `amount <= 0`.
  2. Отправить unsupported `currency`.
  3. Отправить `Guid.Empty`.
- **Expected:** HTTP 400, ошибки валидации по каждому невалидному полю.

### TC-INT-004: PSP unavailable (R-006, EC-4)
- **Тип:** Integration
- **Приоритет:** P0
- **Steps:**
  1. Смоделировать `HttpRequestException` от PSP.
  2. Вызвать `POST /payment/intent`.
  3. Проверить persisted status в DB.
- **Expected:** HTTP 503 с retry hint, intent помечен как `failed`.

### TC-INT-005: MassTransit event payload (FR-5)
- **Тип:** Integration
- **Приоритет:** P1
- **Steps:**
  1. Успешно создать intent.
  2. Проверить опубликованное сообщение.
- **Expected:** опубликован `PaymentIntentCreated` с typed payload (`BookingId`, `IntentId`, `Amount`, `Currency`, `PspPayload`).

### TC-CONF-001: ReturnUrl/CancelUrl из конфигурации (AC-3, R-004)
- **Тип:** Unit + Config
- **Приоритет:** P1
- **Steps:** Прогнать builder tests и проверить передачу `ReturnUrl`/`CancelUrl` в payload.
- **Expected:** в JSON payload присутствуют `returnUrl` и `cancelUrl` из настроек.

### TC-CONF-002: PSP API key из конфигурации (AC-4)
- **Тип:** Config/Manual
- **Приоритет:** P1
- **Steps:** Проверить `PspSettings` binding и отсутствие `demo-key` в коде runtime path.
- **Expected:** ключ читается из `IOptions<PspSettings>`.

### TC-CONF-003: RabbitMQ credentials из конфигурации (AC-7)
- **Тип:** Config/Manual
- **Приоритет:** P1
- **Steps:** Проверить `AddInfrastructure` host setup по `RabbitMqSettings`.
- **Expected:** используются значения из конфигурации, без hardcoded credentials в DI setup.

### TC-CONF-004: Миграция БД (R-002)
- **Тип:** Integration/Manual
- **Приоритет:** P0
- **Steps:**
  1. Запустить миграции на тестовой PostgreSQL.
  2. Проверить схему: `amount bigint`, `provider_ref`, `created_at`, unique index на `booking_id`.
  3. Проверить rollback `Down`.
- **Expected:** миграция применяется и откатывается без data loss сверх ожидаемого.

### TC-NFR-001: API latency p95 < 500ms (NFR-1)
- **Тип:** Performance
- **Приоритет:** P1
- **Steps:** Прогнать нагрузочный сценарий (k6/JMeter) на полном окружении с доступным PSP mock.
- **Expected:** p95 для `POST /payment/intent` < 500ms.

### TC-NFR-002: Sensitive data not logged (NFR-2)
- **Тип:** Security/Observability
- **Приоритет:** P0
- **Steps:** Просмотреть logs при успешном и неуспешном `CreateIntent`.
- **Expected:** отсутствуют значения `Psp:ApiKey` и другие секреты.

## Риски и блокеры выполнения
- В sandbox возможна блокировка NuGet (ошибки `NU1301`, `127.0.0.1:9`) для API/Infrastructure зависимостей.
- При блокировке NuGet интеграционные/конфигурационные проверки, требующие сборки API host, выполняются вручную в полном окружении.
