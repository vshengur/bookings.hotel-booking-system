# Quality

Architecture rules as executable checks — the point is that a violation fails a command,
not that it's written down somewhere and hoped for.

## `arch-tests/` — .NET (ArchUnitNET)

One project per service that has real layering to protect:

```bash
dotnet test quality/arch-tests/BookingsService.ArchTests/BookingsService.ArchTests.csproj
dotnet test quality/arch-tests/PaymentService.ArchTests/PaymentService.ArchTests.csproj
```

No project for pricing-service — it doesn't follow the Domain/Application/Infrastructure
layering the other two do (flat structure, see `services/pricing-service/CLAUDE.md`), so
there's no layer boundary to assert yet.

**If you add a rule and it passes immediately, be suspicious.** ArchUnitNET requires
"positive evaluation" — a namespace filter that matches zero types still reports as failed
with that specific message, which is your signal something's misconfigured, not a real
pass. `ResideInNamespace(x)` is an *exact* match; `ResideInNamespaceMatching(regex)` is
what you want for "this namespace and everything under it"
(`^BookingService\.Domain(\..*)?$`, not `BookingService.Domain.*` or `BookingService.Domain..`
— neither of those does what it looks like they do in this library). Verify a new rule
actually catches something by temporarily breaking it on purpose before trusting a green run.

## Go services — go-arch-lint

One `.go-arch-lint.yml` per service, run from the repo root:

```bash
go-arch-lint check --project-path services/room-service --arch-file .go-arch-lint.yml
go-arch-lint check --project-path services/auth-service --arch-file .go-arch-lint.yml
```

(`--arch-file` is relative to `--project-path`, not the cwd.)

room-service passes clean. **auth-service currently reports one real, known violation**:
`config` depends on `services` (`config.go` calls `services.LoadConsulServiceConfig()`) —
config, which should be the most foundational package, reaches into the services layer to
pull Consul secrets. This is deliberately left failing rather than modeled as allowed —
see `services/auth-service/CLAUDE.md`. Fix the code, don't add an exception to the config.

Install: `go install github.com/fe3dback/go-arch-lint@latest` (needs Go ≥ 1.25 to build the
tool itself — unrelated to the Go 1.23 the services themselves target).

## Not yet set up

- CI wiring (GitHub Actions running both of the above on every PR) — these checks only
  protect the codebase once something actually runs them on every change, not just when a
  human remembers to run them locally.
- pricing-service and dev-gateway have no equivalent checks (pricing-service has no layer
  boundary yet; dev-gateway is nginx config, not code with an architecture to violate).
