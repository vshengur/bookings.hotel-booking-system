# INFRA-003: Оптимизация и стандартизация Dockerfile

## Статус: Open

## Описание
Существующие 8 Dockerfile написаны в разное время, разными людьми, с разными подходами. Необходимо стандартизировать: multi-stage builds, минимальные base images, non-root user, health check, .dockerignore, единый формат labels. Это уменьшит размер images, повысит безопасность и ускорит сборку.

## Acceptance Criteria
- [ ] AC-1: Все Go-сервисы используют multi-stage build: builder (golang:1.23) → runtime (gcr.io/distroless/static или alpine:3.21)
- [ ] AC-2: Все C#-сервисы используют multi-stage build: sdk (mcr.microsoft.com/dotnet/sdk:8.0) → runtime (mcr.microsoft.com/dotnet/aspnet:8.0)
- [ ] AC-3: Все контейнеры запускаются от non-root user
- [ ] AC-4: Каждый Dockerfile имеет HEALTHCHECK instruction
- [ ] AC-5: Каждый сервис имеет `.dockerignore` (bin/, obj/, node_modules/, .git, tests/)
- [ ] AC-6: Единый формат OCI labels: `org.opencontainers.image.source`, `org.opencontainers.image.description`, `org.opencontainers.image.version`
- [ ] AC-7: Размер Go images < 30MB, C# images < 200MB
- [ ] AC-8: Build cache работает — повторная сборка без изменений < 10 секунд

## Функциональные требования
- FR-1: Стандартный шаблон Dockerfile для Go-сервисов
- FR-2: Стандартный шаблон Dockerfile для C#-сервисов
- FR-3: Оптимизация порядка слоёв для максимального cache hit (зависимости → код)
- FR-4: CGO_ENABLED=0 для Go (static binary)
- FR-5: `dotnet publish -c Release --no-restore` для C# (restore отдельным слоем)

## Edge Cases / Corner Cases
- EC-1: Сервисы с gRPC (room-service, pricing-service) — могут требовать дополнительные зависимости для protobuf
- EC-2: payment-service имеет nested project structure (src/PaymentService.API/) — пути в Dockerfile
- EC-3: bookings-service зависит от common/ (Booking.Sharing) — Docker context

## Out of Scope
- Frontend Dockerfile (нет приложений)
- CI/CD image building (INFRA-005)

## Dependencies
- Нет прямых зависимостей (можно делать параллельно с INFRA-001)

## Priority: P1

## Assignee: DevOps-инженер

## Estimated Time: 3-4 часа

## Связь с Roadmap
- IMPLEMENTATION_ROADMAP.md: секция 12 (Infrastructure & DevOps)

## Ключевые файлы
- `services/auth-service/Dockerfile`
- `services/room-service/Dockerfile`
- `services/api-gateway-golang/Dockerfile`
- `services/bookings-service/src/Api/Dockerfile`
- `services/payment-service/src/PaymentService.API/Dockerfile`
- `services/pricing-service/Dockerfile`
- `services/api-gateway/ApiGateway/Dockerfile`
