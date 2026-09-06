# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Distributed microservices hotel booking system targeting 10,000 RPS. Mixed-language: .NET (9 or 10, per service — see Service Map) for business services, Go 1.23+ for auth/room. Currently in PoC phase — some integrations are stubs or partially simulated.

## Repository Structure

True monorepo — all services live as regular directories under `services/`/`frontend/` with full commit history preserved (migrated from git submodules). No `.gitmodules`, no nested `.git` directories. `archive/` holds retired experiments kept for reference only (not built, not part of any active flow) — currently `payment-service-go` and `api-gateway-golang`, both superseded by the canonical .NET `payment-service` and the nginx `dev-gateway`.

## Build & Test Commands

### Infrastructure (start this first)
```bash
cd infra
make up        # Start all infra: RabbitMQ, PostgreSQL x3, Consul, Seq, Redis, MongoDB, ELK, Prometheus/Grafana
make down
make logs
make ps
```
Network: all services attach to external Docker network `booking-net` — create it once with `docker network create booking-net`.

### .NET Services (bookings-service, payment-service, pricing-service, api-gateway)
```bash
dotnet build
dotnet test
dotnet ef migrations add <Name> --project ../Infrastructure   # from API project dir
dotnet ef database update --project ../Infrastructure
docker compose up --build
```

### Go Services (auth-service, room-service)
```bash
go build ./...
go test ./...
go test ./... -v -run TestName   # single test
```

Auth Service gRPC codegen:
```bash
protoc --go_out=. --go-grpc_out=. ../../contracts/proto/auth.proto
```

### Gateway (PoC)
`services/dev-gateway` — nginx reverse proxy, the gateway actually wired into `docker-compose.poc.yml`. No build step; edit `nginx/conf.d/gateway.conf` and restart the container.

`services/api-gateway` (.NET/YARP) exists but is **parked** — not referenced by `docker-compose.poc.yml`. Don't add routing logic there unless explicitly reviving it.

### Kubernetes
```bash
cd infra
make k8s-apply   # kubectl apply -f k8s/
```

## Architecture

### Request Flow
```
Client → dev-gateway (nginx, PoC)
       → Consul service discovery
       → Backend services (REST)
       → Internal gRPC for cross-service calls
       → RabbitMQ for async events (MassTransit sagas)
```

### Service Map

| Service | Lang | Port | DB | Status |
|---|---|---|---|---|
| dev-gateway | nginx | 8080 | — | active (wired in docker-compose.poc.yml) |
| api-gateway | .NET/YARP | 8080 | — | parked, not wired into PoC compose |
| bookings-service | .NET 9 | 5001 | postgres | active |
| payment-service | .NET 9 | 5002 | postgres | active |
| pricing-service | .NET 10 | 5003 | postgres:5434 | active |
| auth-service | Go 1.23 | 5000 | postgres:5432 | active |
| room-service | Go 1.23 | 8083 | postgres:5432 | active |

Archived (see `archive/`, not built/deployed): `payment-service-go` (Go rewrite of payment-service), `api-gateway-golang` (Go rewrite of api-gateway).

### Internal gRPC Contracts (`contracts/proto/`)
None of these are fully wired end-to-end today — verified against actual DI registrations,
not assumed from the .proto files existing:
- `auth.proto` (`AuthService.ValidateToken`) — **unused**. No client or server implementation
  anywhere in the codebase. Both the active dev-gateway (nginx) and the parked api-gateway
  call auth-service's REST `/validate-token` instead (`AuthenticationMiddleware.cs` in
  api-gateway uses a plain `HttpClient`, not this proto).
- `payment.proto` (`PaymentService.{Quote, CreateIntent, Refund}`) — **server unimplemented**.
  payment-service never calls `AddGrpc()`/`MapGrpc*()`, despite generating server scaffolding
  from this proto. A gRPC client exists (`PaymentGatewayGrpc.cs` in bookings-service) but is
  dead code — DI wires `IPaymentGateway` to `PaymentGatewayHttp` (REST) instead. Payment
  intent creation is REST end-to-end; see `contracts/events/asyncapi.yaml` for the async
  result path.
- `availability.proto` (`AvailabilityService.GetOccupancy`) — **client active, server
  unimplemented**. payment-service registers a real `AvailabilityServiceClient`
  (`OccupancyAdapter`, wrapped in `CachedOccupancyService`) and will call it at runtime, but
  no service in this repo implements `AvailabilityServiceBase` — this call has nowhere to
  land unless an external service does.

If you're implementing a new cross-service gRPC call, check whether the .proto already
"exists" before assuming it's live — grep for the generated `*Client`/`*Base` usage and its
DI registration, the way this section was verified.

### Async Messaging (RabbitMQ + MassTransit)
Booking creation uses a **saga** (`BookingStateMachine`, MassTransit state machine in bookings-service, state persisted in MongoDB):
1. `CreateBooking` starts the saga → reserves inventory and calls payment-service **synchronously** (`IPaymentGateway.CreateIntentAsync`, HTTP) to create a payment intent. A 15 min payment timeout is scheduled (`BookingConstants.PaymentTimeout`, via Hangfire message scheduler).
2. payment-service publishes `PaymentStatusChanged` (`Status: succeeded|failed|refunded`) when the PSP settles. bookings-service's `PaymentStatusChangedConsumer` translates this into the saga's own internal `PaymentAuthorized` / `PaymentFailed` events (`refunded` is currently ignored here — cancellation flow owns that transition).
3. On `PaymentAuthorized`: saga requests PMS confirmation (`RequestPmsConfirmationCommand`) and moves to `Reserved`. **No timeout is implemented for this step** — if the PMS never responds, the booking stays in `Reserved` indefinitely (real gap, not the CLAUDE.md-documented "2 min timeout" that older docs claimed — that timeout does not exist in code).
4. On `PmsConfirmed`: saga moves to `Confirmed`.
5. On `PaymentFailed`, payment timeout, or `CancelBooking`: saga triggers `RefundPaymentCommand` and moves to `Failed`/`Cancelled` (compensation).

payment-service also publishes `PaymentIntentCreated` (fire-and-forget notification when an intent is created) — bookings-service does not currently consume it.

Topic exchange pattern (MassTransit default: `{CLR namespace}:{MessageType}`):
- `Bookings.Contracts:*` — saga's own events (`CreateBooking`, `BookingCreated`, `PaymentAuthorized`, `PaymentFailed`, `ConfirmInPms`, `PmsConfirmed`, `CancelBooking`, `BookingCancelled`), defined in `common/Booking.Sharing/Booking.Contracts/Messages.cs`.
- `PaymentService.Application.Messages:*` — the actual payment→bookings wire contracts (`PaymentStatusChanged`, `PaymentIntentCreated`).
- No pricing events exist yet (pricing-service is not wired into the saga; earlier docs mentioning `Prices.Contracts:*` were aspirational, not implemented).

See `contracts/events/asyncapi.yaml` for the full message schema.

### Shared Libraries (`common/Booking.Sharing/`)
- `Booking.Common` — base entities, domain events, value objects
- `Booking.Contracts` — MassTransit message contracts, shared DTOs
Reference via local project path — not a NuGet package.

### Consul
Used for both service discovery and secrets (KV store). Init secrets: `consul-config/init-secrets.sh`. Services register themselves on startup with health check endpoints.

### .NET Service Architecture Pattern
All .NET services follow Clean Architecture:
```
Domain → Application → Infrastructure → API
```
CQRS in bookings-service (MediatR). EF Core migrations live in Infrastructure project.

### Go Service Architecture Pattern
- auth-service, room-service: Gin + GORM/pgx + OpenTelemetry

## Key Configuration

Infra env template: `infra/.env.example`

Critical env vars per service:
- All Go services: `CONSUL_ADDRESS` (service discovery), `DB_*` (postgres)
- auth-service: `JWT_SECRET`, `GOOGLE_CLIENT_ID/SECRET/REDIRECT_URL`
- .NET services: Consul endpoint in `appsettings.json`, EF connection string

Logging: .NET → Serilog → Seq (`http://seq:5341`); Go → Zap JSON stdout.

## PoC Limitations (known stubs)
- Fake PMS (Property Management System) — intentional for PoC
- Inventory/Notification services are placeholders
- Frontend apps are scaffolding only
- Gateway routing not fully aligned with end-to-end user flow
- No timeout on PMS confirmation: a booking that reaches `Reserved` and never gets `PmsConfirmed` stays there forever (only the 15 min payment timeout is actually scheduled)
- Two parked/archived alternative implementations exist for payment-service and api-gateway (Go rewrites) — see Service Map. Don't resurrect them without an explicit decision; the .NET/nginx versions are canonical.
