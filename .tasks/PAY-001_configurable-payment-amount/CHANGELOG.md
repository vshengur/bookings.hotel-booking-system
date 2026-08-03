# Changelog: PAY-001

| Дата | Кто | Действие |
|------|-----|----------|
| 2026-02-21 | PM | Создана задача, приоритет P0 |
| 2026-02-21 | PM | Статус → In Progress, задача передана кодеру |
| 2026-02-21 | Кодер | Начал реализацию, создана ветка feature/PAY-001_configurable-payment-amount |
| 2026-02-21 | Кодер | Анализ завершён, developer-notes.md заполнен |
| 2026-02-21 | Кодер | Реализация: Configuration models, DTOs, Validator, typed Messages |
| 2026-02-21 | Кодер | Реализация: обновлены PSPClient, DI, Controller, appsettings, Program.cs |
| 2026-02-21 | Кодер | Unit-тесты: 13 тестов (validator + builder), все проходят |
| 2026-02-21 | Кодер | Готово к ревью |
| 2026-02-22 | Ревьюер | Ревью #1: REQUEST CHANGES (2 Critical, 4 Major, 1 Minor, 1 Nit) |
| 2026-02-22 | PM | Решения: R-001,R-002,R-004,R-005,R-006 — исправить; R-003,R-007,R-008 — отложить |
| 2026-02-22 | Кодер | Исправил R-001: unique constraint + INSERT-first idempotency |
| 2026-02-22 | Кодер | Исправил R-002: EF Core миграция UpdatePaymentIntentSchema |
| 2026-02-22 | Кодер | Исправил R-004: CancelUrl в PaymentPayloadBuilder и контроллере |
| 2026-02-22 | Кодер | Исправил R-005: PspResponseParser + ProviderRef сохраняется в PaymentIntent |
| 2026-02-22 | Кодер | Исправил R-006: HttpRequestException/timeout → 503 с retry hint |
| 2026-02-22 | Кодер | Новые тесты: 6 для PspResponseParser, обновлён builder тест. Итого 19/19 |
| 2026-02-22 | Кодер | Замечания исправлены, повторный запрос ревью |
| 2026-02-22 | PM | Проверил исправления — все 5 замечаний адресованы. Отправлено на повторное ревью #2 |
| 2026-02-22 | Ревьюер | Ревью #2: REQUEST CHANGES (1 Critical, 1 Major). R-002/R-004/R-006 — accepted, R-001 — reopened, новый R-009 |
| 2026-02-22 | PM | Решения: R-001 (placeholder-first flow) и R-009 (defensive parsing) — исправить. Возвращено кодеру. |
| 2026-02-22 | Кодер | Исправил R-001: placeholder-first flow — INSERT pending → PSP call → UPDATE created. PSP вызывается только после фиксации уникальности |
| 2026-02-22 | Кодер | Исправил R-009: defensive parsing — ValueKind check + GetRawText() fallback для non-string типов |
| 2026-02-22 | Кодер | Новые тесты: 5 для R-009 edge cases (numeric id, object ref, boolean id, null id, array id). Итого 24/24 |
| 2026-02-22 | Кодер | Замечания исправлены, повторный запрос ревью #3 |
| 2026-02-22 | PM | Проверил исправления R-001 и R-009 — оба адресованы корректно. Отправлено на финальное ревью #3 |
| 2026-02-22 | Ревьюер | Ревью #3: APPROVED (R-001 и R-009 исправлены) |
| 2026-02-22 | PM | Код-ревью пройдено. Статус → Testing. Передано тестировщику. |
| 2026-02-22 | Тестировщик | Создан test-plan.md: покрытие AC-1..AC-8, FR-1..FR-5, NFR-1..NFR-2, EC-1..EC-4 и проверок R-001/R-002/R-004/R-005/R-006/R-009 |
| 2026-02-22 | Тестировщик | Проверен unit-suite: 24/24 passed (8 validator, 4 builder, 6 parser, 5 R-009 edge, 1 вспомогательный) |
| 2026-02-22 | Тестировщик | Добавлены integration tests (WebApplicationFactory + test DB + event publish harness) для happy path, idempotency, validation, PSP failure и event publication |
| 2026-02-22 | Тестировщик | Прогон integration tests заблокирован окружением: NU1301 (nuget.org недоступен через proxy 127.0.0.1:9) |
| 2026-02-22 | Тестировщик | Заполнен test-report.md: зафиксированы результаты, coverage assessment и список ручных прогонов для полного окружения |
| 2026-02-22 | PM | Условная приёмка: unit 24/24 pass, code review APPROVED, integration тесты написаны. Блокер — sandbox NuGet (ENV-NUGET-001). |
| 2026-02-22 | PM | Условия merge: integration tests должны пройти в CI/CD до production deployment. |
| 2026-02-22 | PM | Roadmap синхронизирован: IMPLEMENTATION_ROADMAP.md (4.1 done, testing, phase 1, steps) и FRONTEND_ROADMAP.md (API table, контракт, Payment Page) |
