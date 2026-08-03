## Ревью #1 (2026-02-22)

**Вердикт:** REQUEST CHANGES

### R-001
**Файл:** `services/payment-service/src/PaymentService.API/Controllers/PaymentController.cs` (строки 56-88), `services/payment-service/src/PaymentService.Infrastructure/Persistence/PaymentDbContext.cs` (строки 24-27)  
**Уровень:** Critical  
**Категория:** Корректность  
**Проблема:** Идемпотентность реализована как `SELECT` + `INSERT` без атомарности и без уникального ограничения на `BookingId`. При двух параллельных запросах оба потока могут не увидеть запись и создать дубликаты `PaymentIntent`.  
**Почему это плохо:** Для платежей это приводит к двойному созданию intent и потенциально к двойной оплате/двойной оркестрации в downstream-сервисах.  
**Как исправить:** Добавить уникальный индекс/constraint на `booking_id` и обрабатывать `DbUpdateException` (конфликт) с повторным чтением существующего intent. Альтернатива: upsert на уровне БД в транзакции.

### R-002
**Файл:** `services/payment-service/src/PaymentService.Infrastructure/Persistence/PaymentDbContext.cs` (строки 57-66), `services/payment-service/src/PaymentService.API/Program.cs` (строки 61-65)  
**Уровень:** Critical  
**Категория:** Корректность  
**Проблема:** Тип `PaymentIntent.Amount` изменён с `decimal` на `long`, но миграция EF Core отсутствует (в проекте `payment-service` нет каталога миграций).  
**Почему это плохо:** На окружениях с существующей схемой БД приложение получит schema drift: несоответствие модели и таблицы, риски runtime-ошибок и неконсистентных данных.  
**Как исправить:** Создать и применить миграцию для изменения типа колонки и новых полей (`ProviderRef`, `CreatedAt`), проверить обратную совместимость и data conversion strategy.

### R-003
**Файл:** `services/payment-service/src/PaymentService.API/Controllers/PaymentController.cs` (строки 47-50), `FRONTEND_ROADMAP.md` (строка 1372)  
**Уровень:** Major  
**Категория:** Архитектура  
**Проблема:** Контракт `POST /payment/intent` изменён (было только `bookingId`, стало DTO с `amount`/`currency`) без версии API/compat-слоя.  
**Почему это плохо:** Существующие клиенты, отправляющие старый payload, начнут получать 400/415 после деплоя. Это кросс-сервисный breaking change.  
**Как исправить:** Либо добавить версионирование (`/v2/payment/intent`) и временную поддержку старого контракта, либо синхронно обновить все consumers и зафиксировать это как обязательный migration plan перед релизом.

### R-004
**Файл:** `services/payment-service/src/PaymentService.Application/Configuration/PaymentSettings.cs` (строки 7-9), `services/payment-service/src/PaymentService.Application/Builders/PaymentPayloadBuilder.cs` (строки 25-31), `services/payment-service/src/PaymentService.API/Controllers/PaymentController.cs` (строки 69-73)  
**Уровень:** Major  
**Категория:** Корректность  
**Проблема:** `CancelUrl` добавлен в конфигурацию, но нигде не используется при формировании payload в PSP.  
**Почему это плохо:** Acceptance Criteria AC-3 выполнен частично: cancel URL остаётся фактически неинтегрированным в бизнес-флоу.  
**Как исправить:** Расширить `PaymentPayloadBuilder` полем `CancelUrl(...)`, передавать его из `PaymentSettings` в контроллере и покрыть тестом сериализации.

### R-005
**Файл:** `services/payment-service/src/PaymentService.Infrastructure/Persistence/PaymentDbContext.cs` (строки 64-65), `services/payment-service/src/PaymentService.API/Controllers/PaymentController.cs` (строки 77-85)  
**Уровень:** Major  
**Категория:** Корректность  
**Проблема:** Поле `ProviderRef` присутствует в entity, но при создании intent не заполняется из ответа PSP (`pspJson` только публикуется в событии).  
**Почему это плохо:** Нарушается FR-4 (хранение ProviderRef), усложняется трассировка платежей и последующие операции (refund/reconciliation).  
**Как исправить:** Десериализовать ответ PSP в typed DTO, извлечь provider reference/id и сохранять в `PaymentIntent.ProviderRef`.

### R-006
**Файл:** `services/payment-service/src/PaymentService.API/Controllers/PaymentController.cs` (строки 75-76)  
**Уровень:** Major  
**Категория:** Надёжность  
**Проблема:** Ошибки PSP (`EnsureSuccessStatusCode`) не маппятся в доменный ответ `503 Service Unavailable` (EC-4), возвращается generic 500.  
**Почему это плохо:** Клиент не получает корректный сигнал о временной недоступности и стратегии retry, ухудшается устойчивость orchestration flow.  
**Как исправить:** Ловить `HttpRequestException`/таймауты, возвращать `StatusCode(503, ...)` с retry hint; дополнительно логировать корреляционный id.

### R-007
**Файл:** `services/payment-service/tests/Domain.Tests/Validators/CreatePaymentIntentRequestValidatorTests.cs`, `services/payment-service/tests/Domain.Tests/Builders/PaymentPayloadBuilderTests.cs`  
**Уровень:** Major  
**Категория:** Тестируемость  
**Проблема:** Есть только unit-тесты валидатора/билдера, отсутствуют интеграционные тесты для `PaymentController` (контракт API, idempotency, работа с БД, публикация событий, обработка ошибок PSP).  
**Почему это плохо:** Ключевые регрессии в runtime-сценариях (особенно платежи и конкуренция) не ловятся до выката.  
**Как исправить:** Добавить integration/component tests через `WebApplicationFactory` + test DB (или Testcontainers) + MassTransit test harness.

### R-008
**Файл:** `services/payment-service/src/PaymentService.API/Controllers/PaymentController.cs` (строки 52-100)  
**Уровень:** Minor  
**Категория:** Архитектура  
**Проблема:** Значимый объём бизнес-логики (валидация, идемпотентность, вызов PSP, persistence, публикация событий) находится в контроллере.  
**Почему это плохо:** Контроллер быстро станет "толстым", сложнее поддерживать и изолированно тестировать бизнес-правила.  
**Как исправить:** Вынести сценарий `CreateIntent` в application service/handler (например, `CreatePaymentIntentCommandHandler`), контроллер оставить тонким адаптером HTTP.

---

## Проверка акцентов из запроса
- Breaking change `POST /payment/intent`: **да, есть** (R-003), требуется план совместимости/миграции consumers.
- Breaking change `Amount decimal -> long` без миграции: **да, критично** (R-002).
- Бизнес-логика в контроллере: **подтверждено** (R-008).
- Нет интеграционных тестов: **подтверждено** (R-007).
- Полный build не проверен из-за NuGet sandbox: зафиксировано как риск в notes; это увеличивает неопределённость перед релизом.

---

## Ревью #2 (2026-02-22)

**Вердикт:** REQUEST CHANGES

### Статус замечаний из ревью #1
- **R-001:** ❌ Не исправлено полностью (критичный обходной путь сохраняется).
- **R-002:** ✅ Исправлено.
- **R-004:** ✅ Исправлено.
- **R-005:** ⚠️ Частично исправлено (появился новый дефект в парсере, см. R-009).
- **R-006:** ✅ Исправлено.
- **R-003/R-007/R-008:** не проверялись повторно по решению PM.

### R-001 (повторно открыт)
**Файл:** `services/payment-service/src/PaymentService.API/Controllers/PaymentController.cs` (строки 57-71, 99-123), `services/payment-service/src/PaymentService.Infrastructure/Persistence/PaymentDbContext.cs` (строки 24-27)  
**Уровень:** Critical  
**Категория:** Корректность  
**Проблема:** Хотя unique index на `BookingId` добавлен, фактический flow не является `INSERT-first`: вызов PSP (`_psp.CreateIntentAsync`) выполняется **до** попытки `SaveChangesAsync`. При гонке второй запрос тоже создаёт intent у PSP, а конфликт ловится только при записи в БД.  
**Почему это плохо:** Дубликаты в БД предотвращены, но внешний side effect уже произошёл (две PSP intent для одного `BookingId`). Для платежного домена это всё ещё нарушение идемпотентности с финансовым риском.  
**Как исправить:** Перестроить flow так, чтобы уникальность фиксировалась до внешнего вызова (например, запись "pending placeholder" с уникальным `BookingId` в транзакции, затем PSP call и update записи; либо distributed lock/upsert-паттерн). Обязательно исключить повторный PSP вызов при конфликте.

### R-009 (новое)
**Файл:** `services/payment-service/src/PaymentService.Application/Helpers/PspResponseParser.cs` (строки 16-19, 21-23)  
**Уровень:** Major  
**Категория:** Надёжность  
**Проблема:** `ExtractProviderRef()` ловит только `JsonException`, но `GetString()` бросает `InvalidOperationException`, если `id`/`reference` есть, но имеют не string-тип (например, number/object).  
**Почему это плохо:** Нестандартный/изменившийся ответ PSP приведёт к 500 в `CreateIntent` вместо контролируемой деградации.  
**Как исправить:** Проверять `ValueKind == JsonValueKind.String` перед `GetString()` (или расширить обработку типов), и добавить тесты на `{"id":123}` / `{"reference":{...}}`.

## Результат проверки 5 целевых пунктов
1. **R-001:** не принят (критичный путь обхода idempotency через ранний PSP call).
2. **R-002:** принят (миграция `20260222000000_UpdatePaymentIntentSchema` корректно включает conversion, новые колонки, unique index и `Down`).
3. **R-004:** принят (`CancelUrl` сериализуется в payload и покрыт тестом).
4. **R-005:** частично принят (сохранение `ProviderRef` реализовано, но парсер недостаточно устойчив — R-009).
5. **R-006:** принят (разделение `HttpRequestException` и timeout `TaskCanceledException`, возврат 503 с retry hint, structured logging по `BookingId`).

---

## Ревью #3 (2026-02-22)

**Вердикт:** APPROVED

### Статус замечаний из ревью #2
- **R-001:** ✅ Исправлено.
- **R-009:** ✅ Исправлено.
- Остальные замечания (R-002, R-004, R-005, R-006, R-003, R-007, R-008) повторно не проверялись по условиям запроса.

### Проверка R-001 (Critical)
**Файл:** `services/payment-service/src/PaymentService.API/Controllers/PaymentController.cs`, `services/payment-service/src/PaymentService.Infrastructure/Persistence/PaymentDbContext.cs`  
**Статус:** ✅ Принято

Что подтверждено:
- Реализован placeholder-first flow: сначала INSERT `PaymentIntent` со статусом `pending`, затем `SaveChangesAsync` фиксирует unique constraint на `BookingId`.
- При `DbUpdateException` выполняется re-read existing и возврат `200 OK`; PSP в этом пути не вызывается.
- Вызов PSP выполняется только после успешной фиксации уникальности в БД.
- При ошибках PSP (`HttpRequestException`, timeout `TaskCanceledException` при `!ct.IsCancellationRequested`) запись переводится в `failed` и возвращается `503`.

Вывод:
- Критичный путь из ревью #2 закрыт: PSP side-effect больше не происходит до claim уникальности.
- В рамках проверяемых сценариев утечки placeholder со статусом `pending` при ошибках PSP не обнаружено.

### Проверка R-009 (Major)
**Файл:** `services/payment-service/src/PaymentService.Application/Helpers/PspResponseParser.cs`, `services/payment-service/tests/Domain.Tests/Helpers/PspResponseParserTests.cs`  
**Статус:** ✅ Принято

Что подтверждено:
- Добавлен defensive helper `GetStringValue(JsonElement)` с проверкой `ValueKind`.
- Поведение соответствует требованию:
  - `String` -> `GetString()`
  - `Number`/`True`/`False` -> `GetRawText()`
  - `Object`/`Array`/`Null`/`Undefined` -> `null`
- Добавлены и присутствуют тесты на edge cases: numeric id, object reference, boolean id, null id (fallback на reference), array id.

Вывод:
- Сценарий `InvalidOperationException` из ревью #2 устранён; парсер устойчив к non-string JSON типам.