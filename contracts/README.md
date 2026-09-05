# Contracts

Single source of truth for everything that crosses a service boundary. If two services
need to agree on a shape, it lives here — not duplicated in each service's own source.

- `proto/` — shared gRPC contracts (buf-managed). `auth.proto`, `payment.proto`,
  `availability.proto`. Service-internal gRPC (e.g. room-service's own `proto/room.proto`,
  pricing-service's `Protos/pricing.proto`) stays local — it isn't consumed cross-service,
  so centralizing it would just add indirection.
- `events/asyncapi.yaml` — RabbitMQ/MassTransit message contracts (bookings-service ↔
  payment-service saga).
- `openapi/` — REST contracts. See `openapi/README.md` for how each service's spec gets here
  (exported from a live service vs. hand-authored).

## Rule

Change a contract here first, in the same PR as the code that implements or consumes it —
never let a shape drift silently between services. If you're about to hand-copy a DTO shape
across a service boundary instead of referencing what's here, stop and update the contract
instead.
