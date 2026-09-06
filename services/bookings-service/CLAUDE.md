# bookings-service

Saga orchestrator for the booking flow. Clean Architecture: `src/Domain` → `src/Application`
→ `src/Infrastructure` → `src/Api`, plus `src/Contracts` (generated gRPC client code) and
`src/Tests` (xUnit — currently only 3 handler tests: Cancel/Confirm/CreateBooking. No saga
or integration tests exist yet — be extra careful with `BookingStateMachine`, nothing
guards regressions there but manual testing).

## Where things live
- CQRS via MediatR: `Application/Commands` + `Application/Handlers`, one handler per command.
- Saga: `Infrastructure/Messaging/MassTransit/BookingStateMachine.cs`. State persisted in
  MongoDB, not Postgres. Before touching it, read `contracts/events/asyncapi.yaml` — it's
  the source of truth for what `BookingStateMachine` actually consumes/publishes, kept in
  sync by hand (no codegen).
- Cross-service calls: `Application/Abstractions/I*Gateway.cs` interfaces,
  `Infrastructure/Adapters/*Gateway*.cs` implementations. `RoomServiceInventoryGateway` and
  `PaymentGatewayHttp` are the ones DI actually wires up; anything under `Adapters/Simulated/`
  is a PoC stub (`PmsGatewaySimulated`, `InventoryGatewaySimulated`). `PaymentGatewayGrpc.cs`
  is dead code — it compiles and looks real (uses the generated `PaymentServiceClient`) but
  DI never registers it, and payment-service doesn't host a gRPC server anyway. Always check
  `Infrastructure/DependencyInjection.cs` before assuming an adapter is the one actually used.
- Errors: don't add per-controller try/catch. `Api/Middleware/ExceptionHandlingMiddleware.cs`
  already maps `BusinessRuleException` → 4xx, `DbUpdateConcurrencyException` → 409, etc. — add
  new exception types there.
- `RoomId` is `long` end-to-end (domain, DTOs, gateway call). Old EF migrations before
  `20260503132030_NormalizeRoomIdType` show it as `Guid` — that's historical, not current;
  don't copy patterns from migrations older than that one.

## Contracts this service owns/consumes
- Publishes/consumes: `contracts/events/asyncapi.yaml` (`Bookings.Contracts:*` messages).
- Calls room-service over REST: `contracts/openapi/room-service.yaml`.
- Calls payment-service: synchronously via `IPaymentGateway` (HTTP, not proto) for intent
  creation, and consumes `PaymentStatusChanged` (`PaymentService.Application.Messages:*`)
  asynchronously for the result. See `contracts/proto/payment.proto` for the parts of
  payment-service that ARE gRPC (not the intent-creation path).

## Known local gaps
- No timeout on PMS confirmation (`Reserved` state can hang forever) — see root `CLAUDE.md`
  known limitations before "fixing" a booking stuck in `Reserved`; it's a missing feature,
  not a bug in your change.
- No saga integration tests — if you touch `BookingStateMachine`, add one (Testcontainers +
  RabbitMQ) rather than relying on manual verification.
