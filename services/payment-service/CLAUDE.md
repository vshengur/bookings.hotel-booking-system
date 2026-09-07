# payment-service

Nominally Clean Architecture (`src/PaymentService.Domain` → `.Application` →
`.Infrastructure` → `.API`), but **`PaymentService.Domain` has zero source files** — an
empty scaffold project, not a rich domain model. What would normally be domain logic
(builders, validators) actually lives in `.Application`. `tests/Domain.Tests` is
misleadingly named as a result — it references both projects but is really testing
`.Application` code. Don't add real domain logic to the empty `.Domain` project without a
deliberate decision to actually start using it; right now nothing does.

Best-tested service in the repo regardless — `tests/Domain.Tests` (builders/validators)
and `tests/Integration.Tests` (`PaymentApiWebApplicationFactory` + `PaymentIntentApiTests`,
real ASP.NET Core test host). Follow this project's testing pattern as the template for
other .NET services rather than inventing a new one.

## Where things live
- PSP integration: `Infrastructure/PSPClient/PaymentServiceProviderClient.cs` +
  `PaymentRetryPolicy.cs`. `PSP_BASE_URL` env var points at the (simulated) provider.
- Occupancy/availability: `Infrastructure/Occupancy/OccupancyAdapter.cs` wraps a real gRPC
  client (`AvailabilityServiceClient`, from `contracts/proto/availability.proto`) behind
  `CachedOccupancyService` (5 min cache). This client is genuinely wired in DI — but no
  service in this repo implements the server side, so calling it currently has nowhere to
  land. Don't assume it works end-to-end just because the client code looks complete.
- This service also generates gRPC **server** scaffolding from `contracts/proto/payment.proto`
  (`GrpcServices="Server"` in the .csproj) but never calls `AddGrpc()`/maps it in
  `Program.cs`. Payment intent creation is REST-only in practice
  (`PaymentController` / `PaymentIntentService`). Don't wire up the gRPC server without
  first checking whether bookings-service's dead `PaymentGatewayGrpc.cs` client should be
  revived instead of left dead — decide deliberately, don't let both paths half-exist.
- Errors: global `API/Middleware/ExceptionHandlingMiddleware.cs` maps `FluentValidation`
  exceptions to RFC7807 `application/problem+json`. Add new exception types there, not
  per-controller.

## Contracts this service owns/publishes
- Publishes `PaymentIntentCreated` and `PaymentStatusChanged`
  (`PaymentService.Application.Messages:*`) — see `contracts/events/asyncapi.yaml`.
  `PaymentIntentCreated` has no known consumer right now; don't assume something reacts to it.
- `contracts/proto/payment.proto`, `contracts/proto/availability.proto` — see the gRPC
  status notes above before treating either as a working contract.
