# payment-service (Go)

Сервис оплаты и тарифов для системы бронирования. Архитектура — Ports & Adapters (Hexagonal), элементы DDD.
Поддержаны сценарии:
- 3.1 Котировка цены: `GET /payment/quote?roomId=...&start=YYYY-MM-DD&end=YYYY-MM-DD&adults=2&children=0`
- 3.2 Создание Intent: gRPC/REST-заготовка на уровне приложения (см. `internal/app/service.go`), события в RabbitMQ
- 3.3 Capture/verify: HTTP webhook `POST /payment/webhook` с проверкой HMAC, публикация `PaymentSucceeded`/`PaymentFailed`
- 3.4 Refund: метод приложения с ретраями и circuit breaker; REST-эндпойнт можно добросить по аналогии
- 3.5 Ночной перерасчёт тарифов: cron внутри сервиса или K8s CronJob, публикация `PriceListUpdated`

## Быстрый старт (Docker Compose)

```bash
cd build
cp .env.example .env
docker compose up -d --build
# применить миграции и сид
./../scripts/migrate.sh
psql $POSTGRES_URL -f ../scripts/seed_prices.sql
```

Проверка:
- Health:   `curl http://localhost:8080/healthz`
- Metrics:  `curl http://localhost:8080/metrics`
- Quote:    `curl "http://localhost:8080/payment/quote?roomId=R1&start=2025-10-10&end=2025-10-12&adults=2&children=0"`
- Webhook:  `curl -X POST http://localhost:8080/payment/webhook -H "X-PSP-Signature: <hmac>" -d '{"status":"succeeded","intentId":"<id>"}'`

## Структура

- `cmd/payment-service/main.go` — композиция приложения
- `internal/domain/*` — доменные сущности и стратегии тарифов (Стратегия, Строитель для PSP payload, Декоратор/Прокси для ретраев и CB)
- `internal/app/service.go` — application service (use-cases)
- `internal/ports/http/*` — REST (quote/webhook), health, metrics
- `internal/adapters/postgres/*` — pgx-репозитории + миграции
- `internal/adapters/redis/*` — Redis-кэш
- `internal/adapters/rabbitmq/*` — паблишер событий (совместим по JSON-контрактам)
- `internal/adapters/psp/*` — клиент внешнего PSP (retry + circuit breaker) и HMAC
- `internal/scheduler/cron.go` — ночной перерасчёт (можно отключить и использовать K8s CronJob)
- `api/proto/payment.proto` — gRPC-контракт (кодоген не включён по умолчанию, см. Makefile target gen)

## Makefile

```bash
make build
make run
make test
make lint   # если подключите linters
make gen    # с установленным protoc + buf (по желанию)
```

## События (RabbitMQ)

Обменники и тип `topic` (совместимы с MassTransit при биндингах на `Bookings.Contracts:*`):
- `Bookings.Contracts:PaymentIntentCreated`
- `Bookings.Contracts:PaymentSucceeded`
- `Bookings.Contracts:PaymentFailed`
- `Prices.Contracts:PriceListUpdated`

Формат — JSON с полями, описанными в `internal/domain/events.go`.

## gRPC

Файл `api/proto/payment.proto` содержит контракт. В этом минимальном MVP HTTP используется для UI и вебхуков, 
межсервисное взаимодействие — по gRPC после генерации кода (см. `make gen`).

## K8s

Манифесты лежат в `build/k8s/`:
- `deployment.yaml`, `service.yaml`, `configmap.yaml`, `secret.yaml`
- `cronjob.yaml` — расчёт прайса на 30 дней вперёд в 02:30 по расписанию

## Нагрузочные тесты

`k6/quote_load_test.js` — базовый профиль (попробуйте 10k RPS с шардированным Redis и горизонтальным масштабированием).

