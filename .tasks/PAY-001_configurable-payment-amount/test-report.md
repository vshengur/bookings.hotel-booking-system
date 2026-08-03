# Отчёт о тестировании: PAY-001

## Прогон #1 (2026-02-22)

### Summary
- Всего запланировано: 11 ключевых тест-кейсов (unit + integration + config + NFR)
- Выполнено автоматически: 1
- Пройдено: 1
- Провалено: 0
- Заблокировано окружением: 10

### Результаты автоматических прогонов
| Набор | Команда | Результат |
|---|---|---|
| Unit (Domain.Tests) | `dotnet test services/payment-service/tests/Domain.Tests/Domain.Tests.csproj` | ✅ Passed: 24, Failed: 0 |
| Integration (новый набор) | `dotnet test services/payment-service/tests/Integration.Tests/Integration.Tests.csproj` | ⛔ Blocked: restore error `NU1301` (нет доступа к `https://api.nuget.org/v3/index.json`, `127.0.0.1:9`) |

### Проверка обязательного набора unit-тестов (24/24)
- Validator tests: 8/8 ✅
- Builder tests: 4/4 ✅
- PspResponseParser базовые: 6/6 ✅
- R-009 parser edge cases: 5/5 ✅
- Вспомогательный: 1/1 ✅

### Оценка покрытия critical path
- **Что покрыто хорошо (unit):**
  - Валидация DTO (`amount`, `currency`, `bookingId`)
  - Формирование payload (`returnUrl`/`cancelUrl`)
  - Defensive parsing `ProviderRef` (включая non-string JSON)
- **Что ранее не покрывалось и добавлено в integration suite:**
  - API контракт `POST /payment/intent`
  - Idempotency path через БД (placeholder-first)
  - PSP error mapping в `503`
  - Проверка публикации `PaymentIntentCreated`
- **Вывод:** существующих 24 unit-тестов **недостаточно** для полного critical path без integration-слоя; добавленные integration-тесты закрывают пробелы, но их выполнение сейчас заблокировано окружением.

### Блокеры
| ID | Описание | Severity | Статус |
|---|---|---|---|
| ENV-NUGET-001 | Невозможно восстановить пакеты для API/Infrastructure/Integration (`NU1301`, proxy `127.0.0.1:9`) | Critical | Open |

### Что необходимо прогнать вручную в полном окружении
1. `dotnet test services/payment-service/tests/Integration.Tests/Integration.Tests.csproj`
2. `dotnet test services/payment-service/payment-service.sln`
3. Проверка миграции `20260222000000_UpdatePaymentIntentSchema` на PostgreSQL (up/down)
4. NFR-1: нагрузочный тест p95 < 500ms для `POST /payment/intent`
5. NFR-2: аудит логов на отсутствие утечки `Psp.ApiKey`

### Рекомендация QA
- В текущем sandbox: **не готово к финальной приёмке**, т.к. не завершён integration прогон.
- В полном окружении: после успешного прогона integration suite + ручных NFR/миграционных проверок задача может быть переведена в готовность к PM-acceptance.
