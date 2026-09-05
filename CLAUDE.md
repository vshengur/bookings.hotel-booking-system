# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Distributed microservices hotel booking system targeting 10,000 RPS. Mixed-language: .NET 10 for business services, Go 1.23+ for auth/room. Currently in PoC phase — some integrations are stubs or partially simulated.

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
protoc --go_out=. --go-grpc_out=. ../../proto/auth.proto
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
| bookings-service | .NET 10 | 5001 | postgres | active |
| payment-service | .NET 10 | 5002 | postgres | active |
| pricing-service | .NET 10 | 5003 | postgres:5434 | active |
| auth-service | Go 1.23 | 5000 | postgres:5432 | active |
| room-service | Go 1.23 | 8083 | postgres:5432 | active |

Archived (see `archive/`, not built/deployed): `payment-service-go` (Go rewrite of payment-service), `api-gateway-golang` (Go rewrite of api-gateway).

### Internal gRPC Contracts (`proto/`)
- `auth.proto` — `AuthService.ValidateToken` (used by gateway to authenticate requests)
- `payment.proto` — `PaymentService.{Quote, CreateIntent, Refund}`
- `availability.proto` — `AvailabilityService.GetOccupancy`

### Async Messaging (RabbitMQ + MassTransit)
Booking creation uses a **saga** (Automatonymous state machine in bookings-service):
1. Saga emits `PaymentIntentCreated` → payment service
2. Payment responds `PaymentSucceeded` / `PaymentFailed`
3. On success: PMS confirmation with 2 min timeout; payment has 15 min timeout
4. Failed steps trigger compensation (rollback)

Topic exchange pattern: `Bookings.Contracts:*`, `Prices.Contracts:*`

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
- Room ID contract mismatch between bookings-service and room-service (not yet resolved)
- Gateway routing not fully aligned with end-to-end user flow
- Two parked/archived alternative implementations exist for payment-service and api-gateway (Go rewrites) — see Service Map. Don't resurrect them without an explicit decision; the .NET/nginx versions are canonical.
