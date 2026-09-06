# pricing-service

**Does not follow the Domain/Application/Infrastructure/API layering** the other .NET
services use — flat structure: `PricingService/{Controllers,Services,Protos,Middleware}`
in one project. Don't restructure it to match the others as a drive-by change; if it's
worth doing, it's worth its own PR with the resulting layer boundaries covered by an
arch-test, same as bookings-service/payment-service.

**Zero tests exist for this service.** Any non-trivial change here should add coverage —
there's no existing test project to extend, so the first test added also has to set up
the test project.

## Where things live
- `PricingGrpcService.cs` (in `Services/`) implements a **real, mapped** gRPC server
  (`AddGrpc()` + `MapGrpcService<PricingGrpcService>()` in `Program.cs`) from its own local
  `Protos/pricing.proto` — unlike payment-service's payment.proto, this one is actually
  live. It just has **no known consumer**: nothing in bookings-service or elsewhere calls
  it yet (grep for `CalculatePrice`/`PricingGrpc` turns up nothing outside this service).
  If you're wiring bookings-service to pricing-service, this is the contract to use —
  but move `Protos/pricing.proto` into `contracts/proto/` and set up a client the same way
  `RoomServiceInventoryGateway`/`OccupancyAdapter` do it, rather than hand-rolling a new
  gRPC client pattern.
- Uses .NET's native `AddOpenApi()`/`MapOpenApi()` (not Swashbuckle like the other two
  .NET services) — dev-only, same as them.
- `Middleware/ExceptionHandlingMiddleware.cs` — same global-middleware pattern as
  bookings-service/payment-service; extend it there, not per-controller.

## Contracts
- `contracts/proto/` does **not** include this service's proto yet — `pricing.proto` is
  still local (`Protos/pricing.proto`). Move it in the same PR that gives it a real
  cross-service consumer, following the pattern of the other three protos in
  `contracts/proto/` (versioned package, `csharp_namespace`/`go_package` options, buf lint).
