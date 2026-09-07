# room-service

Proper `internal/` package layout (handlers/repository/services/models under
`internal/`, not importable from other modules) — this is the Go layering convention to
copy for other Go services, not auth-service's flat one. `.go-arch-lint.yml` enforces
`handlers → services → repository → models` here and currently passes clean — keep it that
way; don't add a direct `handlers → repository` shortcut even though Go wouldn't stop you.

Has some test coverage (`tests/unit/pricing_strategy_test.go`, `room_builder_test.go`) but
not for the handlers — the three endpoints bookings-service actually calls
(`contracts/openapi/room-service.yaml`) have no test coverage of their own.

## Where things live
- Migrations: versioned SQL files under `migrations/` (`001_...` through `008_...`), not
  GORM auto-migrate. This is the pattern to follow — see auth-service's CLAUDE.md for the
  contrast.
- `RoomID` is `int64` everywhere (DB, DTOs, HTTP path param) — this used to be documented
  as mismatched against bookings-service's `Guid`; it was fixed on the bookings-service
  side (`NormalizeRoomIdType` migration) and this side never needed to change. Don't
  "fix" this again.
- Reservation flow (`ReserveRoom`/`ReleaseRoom` in `internal/handlers/room_handler.go`)
  takes a `booking_reference` string (bookings-service sends the booking GUID as string,
  not a foreign key) — there's no referential integrity between the two services' IDs at
  the DB level, only at the application-call level. Keep that in mind if you're chasing a
  data-consistency bug across the boundary.
- Availability response omits price on purpose (see the comment in
  `internal/models/dto.go`) — pricing is meant to come from pricing-service, which isn't
  wired into this flow yet.

## Contracts
- `contracts/openapi/room-service.yaml` documents only the 3 endpoints bookings-service
  calls, not the full REST surface (search/CRUD also exist but have no known external
  consumer). Extend that file, don't let it silently drift, if you add a new cross-service
  consumer of any other endpoint here.
