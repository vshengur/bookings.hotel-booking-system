# BOOK-002: Интеграция Booking Service с Room Service через gRPC

## Статус: Open

## Описание
Booking Service использует `InventoryGatewaySimulated` — заглушку, которая не делает реальных вызовов. Room Service уже реализован и предоставляет REST API (`POST /api/rooms/{id}/reserve`, `POST /api/rooms/{id}/release`) и gRPC-сервис. Необходимо заменить симуляцию реальным gRPC-клиентом, чтобы при бронировании номер действительно резервировался, а при отмене — освобождался.

## Acceptance Criteria
- [ ] AC-1: `InventoryGatewaySimulated` заменён на `InventoryGatewayGrpc`, использующий gRPC-клиент room-service
- [ ] AC-2: При создании бронирования (Saga: ReserveInventoryCommand) вызывается `POST /api/rooms/{id}/reserve` (или gRPC Reserve) с датами checkIn/checkOut
- [ ] AC-3: При отмене/expire бронирования вызывается `POST /api/rooms/{id}/release` (или gRPC Release)
- [ ] AC-4: Если номер недоступен на указанные даты — бронирование не создаётся, возвращается 409 Conflict
- [ ] AC-5: При недоступности room-service — retry с exponential backoff (3 попытки), затем fail
- [ ] AC-6: gRPC-клиент зарегистрирован в DI, адрес room-service получается из Consul
- [ ] AC-7: Покрыто unit-тестами с мокированием gRPC-клиента

## Функциональные требования
- FR-1: Создать `InventoryGatewayGrpc` — реализация `IInventoryGateway` через gRPC-клиент (proto/room.proto)
- FR-2: ReserveAsync(roomId, checkIn, checkOut) → вызов Reserve RPC
- FR-3: ReleaseAsync(roomId, checkIn, checkOut) → вызов Release RPC
- FR-4: Добавить проверку доступности перед созданием бронирования в CreateBookingCommandHandler
- FR-5: Настроить Consul-based service discovery для gRPC-клиента

## Нефункциональные требования
- NFR-1: gRPC timeout: 5 секунд
- NFR-2: Retry: 3 попытки с exponential backoff (200ms, 400ms, 800ms)
- NFR-3: Circuit breaker при 5 consecutive failures

## Edge Cases / Corner Cases
- EC-1: Room-service недоступен при создании бронирования → вернуть 503 Service Unavailable
- EC-2: Room-service недоступен при отмене → retry, если не удалось — в Dead Letter Queue для ручной обработки
- EC-3: Номер был забронирован другим пользователем между проверкой доступности и резервированием (race condition)
- EC-4: gRPC вызов timeout → retry
- EC-5: Room-service вернул ошибку "room not found" → бронирование Failed

## Out of Scope
- REST-клиент (используем только gRPC)
- Кеширование доступности номеров

## Dependencies
- Room Service gRPC API (proto/room.proto) — уже реализован
- Consul — room-service должен быть зарегистрирован

## Priority: P0

## Assignee: Кодер

## Связь с Roadmap
- IMPLEMENTATION_ROADMAP.md: секция 3.2, задачи "gRPC клиент для Room Service", "ReserveAsync", "ReleaseAsync", "Проверка доступности перед созданием"

## Ключевые файлы
- `services/bookings-service/src/Application/Abstractions/IInventoryGateway.cs`
- `services/bookings-service/src/Infrastructure/Adapters/Simulated/InventoryGatewaySimulated.cs` (заменить)
- `proto/room.proto` (gRPC-контракт room-service)
- `services/room-service/internal/handlers/room_handler.go` (reserve/release endpoints)
