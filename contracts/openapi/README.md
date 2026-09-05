# OpenAPI Contracts

Two different situations, two different approaches — don't hand-author what a service
already generates correctly.

## .NET services (bookings-service, payment-service, pricing-service)

All three already generate an OpenAPI/Swagger document at runtime in development
(`AddSwaggerGen`/`UseSwagger` in bookings-service and payment-service; the native
`AddOpenApi`/`MapOpenApi` in pricing-service). The generated document is the source of
truth for these services' own request/response shapes — don't hand-write a duplicate
here that will drift.

**Not yet done, tracked as follow-up:** a script that starts each service against the
dev infra (`cd infra && make up`) and exports its live spec to this directory
(`contracts/openapi/bookings-service.json`, etc.), so CI can diff it for breaking changes
with `spectral` or `openapi-diff`. Wire this up before treating these three as
CI-enforced contracts.

## Go services (auth-service, room-service)

Neither has an OpenAPI generator wired up (no swaggo or equivalent) — their REST surface
is undocumented. Full specs for every endpoint are not written yet.

`room-service.yaml` in this directory hand-documents only the three endpoints
bookings-service actually calls (`/api/rooms/{id}/availability|reserve|release`), verified
directly against the Go handlers. It is a boundary contract, not a full API reference.

`auth-service.yaml` covers every route dev-gateway exposes at `/api/auth/*` (the proxy
rewrite is a prefix strip, not an allowlist — all of `/login`, `/callback`, `/users`,
`/validate-token` are reachable, not just what the still-scaffolding frontend currently
calls). It also flags a real duplication found while writing it: REST `/validate-token`
and the gRPC `ValidateToken` (`contracts/proto/auth.proto`) are two independent
implementations of the same check with no shared code path — they can silently disagree
if one changes without the other.

If room-service or auth-service grow more cross-service consumers, either extend the
hand-written files per-endpoint as each one gets consumed, or invest in swaggo and export
like the .NET services above — don't let hand-written and generated docs both exist for
the same endpoint.
