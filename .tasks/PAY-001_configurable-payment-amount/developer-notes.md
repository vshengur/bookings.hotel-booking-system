# Developer Notes: PAY-001

## Дата начала: 2026-02-21

### Анализ

**Затрагиваемые файлы (существующие):**
- `services/payment-service/src/PaymentService.API/Controllers/PaymentController.cs` — hardcoded Money(250, "EUR"), return URL
- `services/payment-service/src/PaymentService.Infrastructure/PSPClient/PaymentServiceProviderClient.cs` — hardcoded "demo-key"
- `services/payment-service/src/PaymentService.Infrastructure/DependencyInjection.cs` — hardcoded RabbitMQ "guest"/"guest"
- `services/payment-service/src/PaymentService.Infrastructure/Persistence/PaymentDbContext.cs` — PaymentIntent entity без ProviderRef/CreatedAt
- `services/payment-service/src/PaymentService.API/appsettings.json` — нет секций Payment, Psp
- `services/payment-service/src/PaymentService.API/Program.cs` — нет регистрации IOptions
- `services/payment-service/src/PaymentService.Application/DependencyInjection.cs` — пустой

**Новые файлы:**
- `Application/Configuration/PaymentSettings.cs` — ReturnUrl, CancelUrl, AllowedCurrencies
- `Application/Configuration/PspSettings.cs` — ApiKey, BaseUrl
- `Application/Configuration/RabbitMqSettings.cs` — Host, VirtualHost, Username, Password
- `Application/DTOs/CreatePaymentIntentRequest.cs` — BookingId, Amount (cents), Currency
- `Application/DTOs/CreatePaymentIntentResponse.cs` — IntentId, BookingId, Amount, Currency, Status
- `Application/Validators/CreatePaymentIntentRequestValidator.cs` — FluentValidation
- `Application/Messages/PaymentIntentCreated.cs` — typed MassTransit event
- `Application/Messages/PaymentStatusChanged.cs` — typed MassTransit event
- `tests/Domain.Tests/Validators/CreatePaymentIntentRequestValidatorTests.cs` — 8 тестов валидатора
- `tests/Domain.Tests/Builders/PaymentPayloadBuilderTests.cs` — 3 теста builder
- `tests/Domain.Tests/GlobalUsings.cs` — global using NUnit.Framework

**Зависимости:**
- Bookings.Common (NuGet, v1.0.2) — Money value object, уже была
- FluentValidation.DependencyInjectionExtensions (NuGet, v12.1.1) — добавлена
- FluentAssertions (NuGet, v8.3.0) — добавлена в тесты

**Риски:**
- Изменение сигнатуры `POST /payment/intent` — breaking change (было `Guid`, стало `CreatePaymentIntentRequest`). Booking Service должен адаптироваться.
- Изменение типа `PaymentIntent.Amount` с `decimal` на `long` (cents) — breaking change для БД, требуется миграция.

### Архитектурные решения

**Решение 1: Amount в cents (long)**
Stripe и большинство PSP работают с минимальными единицами валюты (cents). Хранение в cents (long) вместо decimal исключает ошибки округления. Запрос принимает `long Amount` (в центах), PaymentIntent хранит `long Amount`.

**Решение 2: IOptions<T> для всех конфигурационных значений**
PaymentSettings, PspSettings, RabbitMqSettings — всё через IOptions pattern. Это стандартный подход .NET, поддерживает hot-reload, environment variables, secrets.

**Решение 3: FluentValidation вместо DataAnnotations**
Позволяет валидировать допустимые валюты из конфигурации (AllowedCurrencies), что невозможно с атрибутами. Валидатор принимает IOptions<PaymentSettings> через DI.

**Решение 4: Idempotency по BookingId**
Если PaymentIntent для данного BookingId уже существует — возвращаем его (200 OK), а не создаём дубликат. Это покрывает EC-3 из TASK.md.

**Решение 5: Typed MassTransit messages вместо anonymous objects**
Созданы `PaymentIntentCreated` и `PaymentStatusChanged` records. Это обеспечивает типобезопасность, версионирование контрактов и лучшую поддержку в consumers.

**Альтернатива, которую отверг:**
- Вынесение бизнес-логики в отдельный Application Service (IPaymentService) — решил не делать в рамках этой задачи, т.к. логика пока простая (1 action). Если контроллер разрастётся — рефакторинг в отдельную задачу.

### Ход реализации

#### Итерация 1 (2026-02-21)
1. Создана ветка `feature/PAY-001_configurable-payment-amount` от `origin/main`
2. Созданы Configuration models: PaymentSettings, PspSettings, RabbitMqSettings
3. Созданы DTOs: CreatePaymentIntentRequest, CreatePaymentIntentResponse
4. Созданы typed messages: PaymentIntentCreated, PaymentStatusChanged
5. Добавлен FluentValidation + CreatePaymentIntentRequestValidator (с IOptions<PaymentSettings>)
6. Обновлён PaymentIntent entity: добавлены ProviderRef, CreatedAt; Amount → long
7. Обновлён PSPClient: API key из IOptions<PspSettings> вместо "demo-key"
8. Обновлён Infrastructure DI: RabbitMQ creds из RabbitMqSettings вместо hardcoded "guest"/"guest"
9. Обновлён PaymentController: принимает DTO, валидирует, сохраняет PaymentIntent в БД, использует IOptions для return URL, публикует typed events
10. Обновлены appsettings.json/Development.json: добавлены секции Payment, Psp, обновлён RabbitMq
11. Обновлён Program.cs: регистрация IOptions для всех секций
12. Написаны unit-тесты: 13 тестов (8 валидатор, 3 builder, 2 вспомогательных), все проходят
13. Обновлён тестовый проект: добавлены FluentAssertions, Microsoft.Extensions.Options, ImplicitUsings

**Проблемы:**
- Application проект не имел ImplicitUsings → добавил явные `using System;` в DTOs/Messages
- FluentValidation 12.x убрал `WithMessageContaining` из TestHelper → упростил assertion
- FluentAssertions 8.x: `HaveCountGreaterOrEqualTo` → `HaveCountGreaterThanOrEqualTo`
- Sandbox-среда блокирует NuGet для Infrastructure/API проектов (прокси 127.0.0.1:9) — тесты Application слоя проходят, полный solution build нужно проверить локально

#### Итерация 2 (2026-02-22) — после ревью #1

Замечания из review-comments.md, решения PM из pm-acceptance.md.

**Исправлено:**

1. **R-001 (Critical)**: Idempotency переделана на INSERT-first + catch `DbUpdateException`.
   - `BookingId` index → `IsUnique()` в OnModelCreating
   - Контроллер: сначала INSERT, при конфликте — detach entity, re-read existing через `AsNoTracking()`, вернуть 200 OK
   - Убрана race condition: уникальный constraint на уровне БД гарантирует атомарность

2. **R-002 (Critical)**: Создана EF Core миграция `20260222000000_UpdatePaymentIntentSchema`:
   - `amount`: numeric → bigint с конвертацией `(amount * 100)::bigint`
   - Добавлены колонки `provider_ref` (varchar(256), nullable) и `created_at` (timestamptz, default now())
   - Индекс `ix_payment_intents_booking_id`: drop non-unique → create unique
   - Down-метод для обратимости

3. **R-004 (Major)**: `CancelUrl` интегрирован:
   - Добавлен метод `PaymentPayloadBuilder.CancelUrl(string)` → сериализуется в `cancelUrl`
   - Контроллер передаёт `_paymentSettings.CancelUrl` в builder
   - Обновлён builder тест на проверку `cancelUrl` в JSON

4. **R-005 (Major)**: `ProviderRef` заполняется:
   - Создан `PspResponseParser.ExtractProviderRef(string)` в Application/Helpers — ищет `id`, затем `reference` в JSON
   - Контроллер вызывает parser, сохраняет результат в `intent.ProviderRef` перед `SaveChangesAsync`
   - 6 unit-тестов для parser (id, reference, both, missing, invalid JSON, empty)

5. **R-006 (Major)**: Ошибки PSP → 503:
   - `HttpRequestException` → 503 с `RetryAfterSeconds: 5`
   - `TaskCanceledException` (timeout, не client cancellation) → 503 с `RetryAfterSeconds: 10`
   - Structured logging с BookingId для обоих случаев

**Не исправлено (по решению PM):**
- R-003 (API versioning) — допускается на MVP, consumers пока нет
- R-007 (интеграционные тесты) — задача тестировщика
- R-008 (бизнес-логика в контроллере) — техдолг, отнесено в ARCH-001

**Тесты:** 19/19 проходят (13 прежних + 6 новых PspResponseParser)

#### Итерация 3 (2026-02-22) — после ревью #2

Замечания из review-comments.md (секция «Ревью #2»), решения PM из pm-acceptance.md (секция «Решения по ревью #2»).

**Исправлено:**

1. **R-001 (Critical, reopened)**: Flow полностью перестроен на placeholder-first паттерн.
   - **Проблема**: PSP вызывался ДО SaveChangesAsync — при гонке двух запросов оба создавали intent у PSP, хотя в БД сохранялся только один. Двойной финансовый side-effect.
   - **Решение**: 3 фазы:
     1. INSERT placeholder (Status=`pending`) → `SaveChangesAsync` фиксирует unique constraint на BookingId
     2. При `DbUpdateException` — re-read existing, вернуть 200 OK. PSP **не вызывается**.
     3. Если INSERT успешен — вызов PSP, затем UPDATE: Status=`created`, ProviderRef из ответа PSP
   - При ошибке PSP (HttpRequestException/timeout) — intent обновляется в Status=`failed`, возвращается 503
   - **Ключевое**: внешний side-effect (PSP) происходит ТОЛЬКО после фиксации уникальности в БД

2. **R-009 (Major, new)**: Defensive parsing в PspResponseParser.
   - **Проблема**: `GetString()` бросает `InvalidOperationException` на non-string типах (`{"id": 123}`, `{"reference": {...}}`)
   - **Решение**: Добавлен private метод `GetStringValue(JsonElement)` с проверкой `ValueKind`:
     - `String` → `GetString()`
     - `Number`, `True`/`False` → `GetRawText()` (fallback для числовых/boolean id)
     - `Object`, `Array`, `Null`, `Undefined` → `null` (пропуск)
   - 5 новых тестов: numeric id (`123`), object reference, boolean id, null id с fallback на reference, array id

**Не исправлено (по решению PM):**
- Всё из ревью #1 (R-003, R-007, R-008) — статус не изменился

**Тесты:** 24/24 проходят (19 прежних + 5 новых R-009 edge cases)

### Технический долг
- **PAY-002**: Webhook HMAC signature validation (оставлен TODO)
- **PAY-003**: Перевод PaymentIntent.Status со строк на enum (`PaymentIntentStatus { Pending, Created, Failed, Completed, Refunded, Cancelled }`). Формализует state machine, исключает ошибки из-за опечаток в строковых статусах. Требуется EF Core миграция (varchar → int) и обновление JSON-сериализации в MassTransit events (JsonStringEnumConverter).
- **R-008/ARCH-001**: Контроллер содержит бизнес-логику — вынести в Application Service при следующем рефакторинге
- **R-007**: Интеграционные тесты (WebApplicationFactory + InMemory DB) — задача тестировщика
- Нет интеграционных тестов для PaymentController (требует WebApplicationFactory + InMemory DB)
- Миграция создана вручную (sandbox не позволял `dotnet ef`) — нужно провалидировать через `dotnet ef migrations list` на полном окружении
