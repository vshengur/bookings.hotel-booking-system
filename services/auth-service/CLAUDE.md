# auth-service

**Flat package layout** (`handlers/`, `models/`, `repository/`, `services/`, `utils/` all
at the module root) — unlike room-service, nothing here is under `internal/`, so anything
in this module is technically importable by other Go modules in the repo. Don't rely on
that; treat package boundaries as intended even though Go won't enforce it here.

**Zero test files exist** (`*_test.go`) — no test project to extend, same caveat as
pricing-service: the first test added also sets up the pattern for the rest.

## Where things live
- `database/*.go` calls `db.AutoMigrate(&models.User{})` on startup — GORM's auto-migrate,
  not a versioned migration tool. Fine for the PoC's single `User` model; don't reach for
  this pattern if the schema gets more complex — room-service uses versioned SQL files
  under `migrations/` instead, that's the pattern to follow for anything bigger than this.
- Two independent implementations of "is this token valid": REST `handlers/token.go`
  (`GET /validate-token`) and the unused gRPC `contracts/proto/auth.proto` — see root
  `CLAUDE.md`'s gRPC Contracts section. If you fix a bug in one, check whether the other
  needs the same fix; they share no code.
- `services/consul.go` — service registration/discovery. `CONSUL_FOLDER` env var picks the
  KV namespace for secrets (default `config`).
- **`config/config.go` depends on `services`** (`services.LoadConsulServiceConfig()`,
  `services.GetConsulSecret()`) — a real layering inversion: config is meant to be the most
  foundational package, but it reaches into `services` to pull secrets from Consul.
  `.go-arch-lint.yml` deliberately does *not* allow this and will report it — that's a
  known, currently-failing check, not a bug in the linter config. Fix by moving the Consul
  KV client itself below `config` (or into `config`) rather than adding an exception.
- `models/user.go`: `UserID` (string UUID) is the cross-service guestId — reference that,
  not the GORM-internal numeric `ID`, if another service ever needs to key off a user.

## Contracts this service exposes
- `contracts/openapi/auth-service.yaml` — every route dev-gateway proxies at `/api/auth/*`
  (`/login`, `/callback`, `/validate-token`, `/users*`). Keep it in sync with
  `routes/routes.go` — this file is currently the only documentation of this REST surface.
- `contracts/proto/auth.proto` — exists but unimplemented here (no generated server, no
  handler). Don't add a REST endpoint AND wire this proto for the same capability without
  deciding which one is canonical first (see the duplication note in the openapi file).
