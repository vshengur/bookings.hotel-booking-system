# Hotel Booking System - Actual Status and PoC Checklist

## Purpose

This document is a practical snapshot of the repository state as of 2026-05-03.

It is intended to answer:

- What is already implemented in code
- What exists only partially or behind stubs
- What is still not started
- What must be completed before a real PoC demo

It complements, but does not replace:

- `IMPLEMENTATION_ROADMAP.md`
- `FRONTEND_ROADMAP.md`

---

## Status Legend

- `Implemented` - present in code and looks usable
- `Partial` - important parts exist, but the feature is incomplete
- `Stub/Placeholder` - directory, interface, or endpoint exists, but not a working implementation
- `Not Started` - no real implementation found
- `PoC Critical` - required for a believable end-to-end booking demo

---

## Executive Summary

Current stage: `Backend foundation / integration pre-PoC`

What is strong already:

- Auth foundation exists
- Room catalog and availability service are in good shape
- Pricing service exists and is relatively mature
- Booking and payment services are real services, not empty scaffolding
- Infrastructure baseline exists: Consul, RabbitMQ, PostgreSQL, monitoring, gateway variants

What blocks PoC today:

- Booking inventory integration is still simulated
- Booking still depends on simulated integrations in key places
- Gateway routing is not yet aligned to the final user-facing flow
- Frontend applications are still placeholders
- Notification and guest-facing supporting services are not ready
- There is a contract mismatch between booking room identifiers and room-service room identifiers

Bottom line:

The project is beyond architecture/setup stage, but not yet at working end-to-end PoC stage.

---

## Repository-Level Reality Check

### Present and active

- `services/auth-service`
- `services/room-service`
- `services/pricing-service`
- `services/bookings-service`
- `services/payment-service`
- `services/payment-service-go`
- `services/api-gateway`
- `services/api-gateway-golang`
- `services/consul`
- `infra`

### Present but effectively placeholders or minimal

- `services/notification-service`
- `services/inventory-service`
- `frontend/booking-frontend`
- `frontend/payment-frontend`
- `frontend/notification-frontend`

### Structural note

There is naming drift in the repository:

- submodule path `services/booking-service`
- active codebase path `services/bookings-service`

This is not an immediate PoC blocker, but it is a maintenance risk and source of confusion.

---

## Actual Status by Area

### 1. Authentication

Status: `Implemented`
PoC Critical: `Yes`

What exists:

- `auth-service` is present and wired as a real service
- repository docs and prior roadmap indicate Google OAuth and JWT-based auth flow
- Consul integration around auth is documented and appears actively used

Confidence level:

- high enough for PoC planning

Notes:

- Auth is not the main current blocker

---

### 2. Room Search and Availability

Status: `Implemented`
PoC Critical: `Yes`

What exists:

- room CRUD
- search API
- availability API
- reserve/release operations
- migrations and seed data
- gRPC surface
- README with concrete endpoints and local run instructions

Assessment:

- one of the most complete services in the repo

PoC gap:

- route this cleanly through the chosen API gateway

---

### 3. Pricing

Status: `Implemented`
PoC Critical: `Useful, but can be simplified`

What exists:

- REST API for price calculation
- pricing rules and strategies
- gRPC service
- database-backed implementation
- controller-level exception handling moved into API middleware

Assessment:

- mature enough to support PoC

PoC gap:

- ensure booking/payment consume real pricing consistently

---

### 4. Booking Service

Status: `Partial`
PoC Critical: `Yes`

What exists:

- create booking endpoint
- get booking endpoint
- confirm booking endpoint
- cancel booking endpoint
- domain model and persistence
- saga/state machine
- MassTransit integration
- timeout-related scheduling concepts
- basic handler-level tests for confirm/cancel flow
- optimistic concurrency protection has started at the persistence layer for booking updates
- controller-level exception handling moved into API middleware

What is incomplete in code:

- no visible complete user bookings listing API
- critical inventory integration is still simulated
- booking creation validation is still incomplete

Evidence in code:

- `services/bookings-service/src/Api/Controllers/BookingController.cs`
- `services/bookings-service/src/Application/Handlers/ConfirmBookingCommandHandler.cs`
- `services/bookings-service/src/Application/Handlers/CancelBookingCommandHandler.cs`
- `services/bookings-service/src/Infrastructure/Persistence/BookingDbContext.cs`

Assessment:

- the service foundation is real
- confirm/cancel are no longer stubs
- the externally usable booking workflow is still not finished because room reservation is not yet real

PoC gap:

- inventory integration remains the main backend blocker

Concurrency note:

- booking updates can still collide across parallel requests and saga-driven status changes
- optimistic concurrency protection has been started for booking writes, but this area still needs broader validation

---

### 5. Booking Integrations

Status: `Partial / simulated`
PoC Critical: `Yes`

#### Inventory / room reservation integration

Status: `Stub/Placeholder`

What exists:

- simulated inventory gateway

Evidence:

- `bookings-service` now uses `long` room identifiers compatible with `room-service`
- a real room-service-backed inventory gateway now exists in bookings-service
- room-service reservation contract was updated to use `booking_reference` instead of forcing numeric booking ids

PoC requirement:

- finish validating the real room-service integration for availability, reserve, release

Current blocker:

- availability pre-check and full end-to-end smoke validation are still pending

#### PMS integration

Status: `Stub/Placeholder`

What exists:

- simulated PMS gateway publishes delayed fake confirmation

PoC requirement:

- for PoC, real PMS is optional
- simulated PMS is acceptable if the booking flow otherwise works end-to-end

Assessment:

- inventory integration is critical now
- PMS can remain simulated for the first demo

---

### 6. Payment Service

Status: `Partial, but substantial`
PoC Critical: `Yes`

What exists:

- payment intent endpoint
- refund endpoint
- webhook endpoint
- persistence for payment intents
- idempotency pattern around `BookingId`
- validation
- typed event publishing
- unit and integration test projects
- create-intent orchestration is no longer embedded in controller-level `try/catch`
- controller-level exception handling moved into API middleware

What is still incomplete:

- webhook signature validation is still TODO
- refund flow is basic
- some booking/payment coupling still needs alignment

Evidence in code:

- webhook contains `TODO(PAY-002): validate HMAC signature`

Assessment:

- this is much closer to usable than empty
- probably good enough for a test-mode PoC after integration cleanup

PoC gap:

- verify that booking amount, intent creation, webhook update, and booking status transitions actually work together

---

### 7. API Gateway

Status: `Partial`
PoC Critical: `Yes`

What exists:

- C# gateway with YARP
- Go gateway with basic route wiring
- JWT/rate limiting/service discovery concepts present

What is incomplete:

- no evidence of complete user-facing routing layout for:
  - rooms
  - bookings
  - pricing
  - payment

Evidence:

- C# gateway route config currently only shows auth-focused routes
- Go gateway route registration is still generic and minimal

Assessment:

- gateway exists technically
- it is not yet clearly set up as the single clean entry point for the product flow

PoC gap:

- unify routing for the demo flow

---

### 8. Frontend

Status: `Placeholder`
PoC Critical: `Yes`

What exists:

- frontend directories and submodule entries
- roadmaps describing intended architecture

What does not exist yet:

- real guest-facing application code
- search page
- room details page
- booking form
- payment screen
- confirmation screen

Evidence:

- frontend repos currently contain only minimal placeholder files like README and LICENSE

Assessment:

- there is no product-facing PoC without at least a minimal frontend or demo UI

PoC gap:

- biggest product-level blocker after booking integration

---

### 9. Notification Service

Status: `Placeholder`
PoC Critical: `No`

What exists:

- directory only

PoC assessment:

- can be skipped for first PoC
- confirmation can be shown in UI instead of delivered by email

---

### 10. Guest Service

Status: `Not Started`
PoC Critical: `No`

What exists:

- no active service implementation found

PoC assessment:

- can be deferred
- guest information can be captured directly in booking for initial demo

---

### 11. Testing

Status: `Partial`
PoC Critical: `Yes, but only to smoke-test level`

What exists:

- payment service has real test projects
- room service has at least some unit tests
- bookings service has a test project structure

What is missing for confidence:

- verified end-to-end integration path across room -> booking -> payment -> confirmation
- clear smoke-test checklist for local demo environment

Assessment:

- some service-level testing exists
- system-level PoC validation is still missing

---

### 12. Infrastructure and DevOps

Status: `Implemented baseline / Partial operationalization`
PoC Critical: `Yes, baseline only`

What exists:

- infra folder
- docker-compose infrastructure
- Consul
- monitoring stack
- data stores and queueing

What is not required for first PoC:

- production-grade CI/CD
- full Kubernetes rollout
- full alerting

Assessment:

- enough baseline exists to run a PoC environment

---

## Consolidated Status Table

| Area | Actual State | PoC Critical | Notes |
|---|---|---:|---|
| Auth | Implemented | Yes | Not a major blocker |
| Room Service | Implemented | Yes | Strongest backend area |
| Pricing Service | Implemented | Optional/Helpful | Can support demo pricing |
| Booking API | Partial | Yes | Confirm/cancel implemented, booking validation still incomplete |
| Booking Inventory Integration | Partial | Yes | Real adapter exists, but end-to-end reserve/release still needs smoke validation |
| PMS Integration | Stub/Placeholder | No | Can stay simulated for PoC |
| Payment Service | Partial but substantial | Yes | Webhook hardening still pending |
| API Gateway | Partial | Yes | Needs real demo routes |
| Frontend | Placeholder | Yes | No real UI yet |
| Notification Service | Placeholder | No | Can defer |
| Guest Service | Not Started | No | Can defer |
| Integration Testing | Partial | Yes | Need smoke-tested flow |
| Infra Baseline | Implemented baseline | Yes | Enough for demo environment |

---

## Minimum Scope for PoC

PoC should prove one believable happy path:

1. User authenticates
2. User searches available rooms
3. User opens room details
4. User creates a booking
5. System reserves the room
6. User pays in test mode
7. Booking status advances correctly
8. User sees final confirmation

For first PoC, the following are acceptable shortcuts:

- PMS stays simulated
- email notifications are skipped
- guest profile service is skipped
- admin UI is skipped
- advanced security hardening is partial

---

## Prioritized PoC Backlog

### P0 - Must be done before demo

1. Finish booking API behavior
   - booking confirm and cancel are implemented
   - add missing validation around booking creation and state transitions

2. Replace simulated inventory integration
   - finish validating bookings-service to room-service integration
   - check availability before booking
   - reserve room on booking creation
   - release room on cancel/expire/failure

3. Stabilize booking-payment flow
   - ensure booking amount is real
   - ensure payment intent is created from booking data
   - ensure webhook updates booking state correctly
   - keep middleware-based error mapping consistent across booking, pricing, and payment APIs

4. Wire gateway routes for demo
   - auth
   - rooms
   - bookings
   - pricing
   - payment

5. Build minimal frontend
   - search page
   - room details page
   - booking form
   - payment page
   - confirmation page

6. Run an end-to-end smoke test in local environment

### P1 - Strongly recommended right after PoC path works

1. Add webhook signature validation
2. Add user bookings list endpoint
3. Validate optimistic concurrency behavior under competing booking updates
4. Add basic demo seed/setup instructions
5. Add one scripted smoke-test checklist

### P2 - Can wait until after PoC

1. Notification service
2. Guest service
3. Real PMS integration
4. Admin frontend
5. Production hardening
6. Kubernetes/CI/CD maturity

---

## Suggested 1-2 Week Execution Order

### Track A - Backend core

1. Finish booking controller and command flow
2. Replace simulated inventory gateway
3. Validate saga transitions with real payment events

### Track B - Platform entry point

1. Finalize gateway routes
2. Normalize local run path through gateway

### Track C - Demo UI

1. Create minimal guest frontend
2. Hook search and booking flow to backend
3. Use payment test mode

### Track D - Validation

1. Smoke test happy path
2. Fix status mismatches and integration bugs
3. Document exact demo steps

---

## Definition of Done for PoC

PoC is done when all of the following are true:

- a user can search rooms from a frontend or demo UI
- a user can create a booking against a real available room
- the system performs a real reserve/release interaction with room-service
- a payment intent can be created in test mode
- payment completion updates booking status correctly
- the user sees a final confirmation page/state
- the flow can be repeated locally by another developer with written steps

---

## Recommended Next Actions

If this document becomes the new source of truth, the next best steps are:

1. Keep this file as the factual repo-status document
2. Update `IMPLEMENTATION_ROADMAP.md` to match this reality
3. Derive a concrete engineering task list from the P0 backlog

---

## Sources Used

This document was derived by cross-checking:

- `IMPLEMENTATION_ROADMAP.md`
- `FRONTEND_ROADMAP.md`
- service READMEs
- actual repository structure
- key code paths in booking, payment, gateway, and frontend directories
