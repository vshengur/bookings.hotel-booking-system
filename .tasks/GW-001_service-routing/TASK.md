# GW-001: Добавить routing для всех сервисов в API Gateway

## Статус: Open

## Описание
API Gateway (C# YARP) сейчас проксирует только auth-service. Room Service, Booking Service, Pricing Service и Payment Service недоступны через gateway. Фронтенд не может работать без единой точки входа ко всем API. Необходимо добавить маршруты ко всем существующим сервисам, настроить CORS для фронтенда и определить, какие endpoints требуют аутентификации.

## Acceptance Criteria
- [ ] AC-1: `/api/rooms/**` проксируется в room-service
- [ ] AC-2: `/api/booking/**` проксируется в bookings-service
- [ ] AC-3: `/api/pricing/**` проксируется в pricing-service
- [ ] AC-4: `/payment/**` проксируется в payment-service
- [ ] AC-5: Публичные endpoints (search, room details, webhook) доступны без JWT
- [ ] AC-6: Защищённые endpoints (create booking, cancel, profile) требуют валидный JWT
- [ ] AC-7: CORS настроен для `http://localhost:3000` (dev frontend) и конфигурируемых origins
- [ ] AC-8: Все сервисы обнаруживаются через Consul (динамический routing)
- [ ] AC-9: Health check gateway проверяет доступность всех backend-сервисов

## Функциональные требования
- FR-1: Добавить route configuration в appsettings.json для каждого сервиса
- FR-2: Маршруты с правилами аутентификации:
  - Public: GET /api/rooms/*, GET /api/rooms/search, POST /payment/webhook, GET /api/pricing/calculate
  - Protected: POST/PUT/DELETE /api/rooms/* (admin), POST /api/booking/*, GET /api/booking/*, POST /payment/intent
- FR-3: CORS middleware с конфигурируемыми allowed origins, methods, headers
- FR-4: Сервисы регистрируются в Consul с правильным `prefix` metadata для dynamic routing
- FR-5: Correlation ID header (X-Correlation-Id) добавляется к каждому проксированному запросу

## Нефункциональные требования
- NFR-1: Latency overhead gateway < 50ms
- NFR-2: Gateway не кеширует ответы backend-сервисов (stateless proxy)
- NFR-3: Логирование каждого проксированного запроса (method, path, upstream, status, duration)

## Edge Cases / Corner Cases
- EC-1: Backend-сервис недоступен → 503 Service Unavailable (не 500)
- EC-2: Backend-сервис не зарегистрирован в Consul → 503
- EC-3: Запрос к несуществующему маршруту → 404
- EC-4: JWT expired → 401 (не проксировать запрос)
- EC-5: Preflight CORS request (OPTIONS) → 200 с правильными headers

## Out of Scope
- Go Gateway (api-gateway-golang) — фокусируемся на C# YARP
- Per-user rate limiting (уже есть global, per-user — позже)
- Circuit breaker (отдельная задача)

## Dependencies
- Все backend-сервисы должны быть зарегистрированы в Consul с `prefix` metadata
- Auth-service должен быть доступен для JWT validation

## Priority: P0

## Assignee: Кодер

## Связь с Roadmap
- IMPLEMENTATION_ROADMAP.md: секция 8.1 "Добавить routing для новых сервисов" и секция 8.3 "CORS Configuration"

## Ключевые файлы
- `services/api-gateway/Program.cs`
- `services/api-gateway/appsettings.json` (route config)
- `services/api-gateway/Middleware/AuthenticationMiddleware.cs`
