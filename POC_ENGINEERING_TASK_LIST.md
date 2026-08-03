# Hotel Booking System - PoC Engineering Task List

## Purpose

This document turns `PROJECT_STATUS_POC.md` into a practical engineering queue.

It is focused on:

- concrete PoC-critical tasks
- likely files and services to touch
- expected outcome of each task
- recommended execution order

This is not a product roadmap. It is an implementation checklist for getting to a working PoC.

---

## PoC Goal

Deliver one working happy path:

1. User authenticates
2. User searches rooms
3. User opens room details
4. User creates booking
5. System reserves room
6. User pays in test mode
7. Booking status updates correctly
8. User sees confirmation

---

## Priority Legend

- `P0` - must be completed before PoC demo
- `P1` - highly recommended immediately after happy path works
- `P2` - can wait until after PoC

---

## Workstream A - Booking Service

Cross-cutting status:

- API exception handling has been moved from booking controllers to middleware

### A1. Implement booking confirm endpoint

Priority: `P0`
Status: `Done`

Service:

- `services/bookings-service`

Main files:

- `services/bookings-service/src/Api/Controllers/BookingController.cs`
- `services/bookings-service/src/Application/Commands/ConfirmBookingCommand.cs`
- `services/bookings-service/src/Application/Handlers/`
- `services/bookings-service/src/Domain/Aggregates/Booking/Booking.cs`

What to do:

- replace controller stub with real command dispatch
- implement command handler if missing/incomplete
- add allowed state transition rules
- return meaningful HTTP result

Done when:

- `POST /api/booking/{id}/confirm` changes booking state correctly
- invalid state transitions are rejected

---

### A2. Implement booking cancel endpoint

Priority: `P0`
Status: `Done`

Service:

- `services/bookings-service`

Main files:

- `services/bookings-service/src/Api/Controllers/BookingController.cs`
- `services/bookings-service/src/Application/Commands/CancelBookingCommand.cs`
- `services/bookings-service/src/Application/Handlers/`
- `services/bookings-service/src/Domain/Aggregates/Booking/Booking.cs`

What to do:

- replace controller stub with real command dispatch
- support cancel reason
- ensure cancel triggers proper downstream behavior

Done when:

- `POST /api/booking/{id}/cancel` moves booking to cancelled flow
- cancellation releases inventory and handles payment refund path appropriately

Current note:

- inventory release currently goes through the existing gateway abstraction, but that gateway is still simulated

---

### A3. Add booking creation validation

Priority: `P0`
Status: `In progress`

Service:

- `services/bookings-service`

Main files:

- `services/bookings-service/src/Api/Controllers/BookingController.cs`
- `services/bookings-service/src/Application/Commands/CreateBookingCommand.cs`
- `services/bookings-service/src/Application/Handlers/CreateBookingCommandHandler.cs`

What to do:

- validate `checkIn < checkOut`
- validate `checkIn` is not in the past
- validate booking has at least one line item
- validate positive nights and money values
- keep the new real inventory availability pre-check in the create flow

Done when:

- invalid requests fail early with usable API errors

Current note:

- create flow now rejects past check-in dates
- create flow now rejects empty item lists
- create flow now rejects non-positive nightly prices
- remaining gaps are guest existence and promo-code validation

---

### A4. Add user bookings list endpoint

Priority: `P1`

Service:

- `services/bookings-service`

Main files:

- `services/bookings-service/src/Api/Controllers/BookingController.cs`
- repository/query layer files in `Application` and `Infrastructure`

What to do:

- add endpoint like `GET /api/bookings?guestId=...`
- support basic filtering by status
- add simple pagination if easy

Done when:

- frontend can show a minimal "My bookings" page after PoC happy path exists

---

## Workstream B - Booking Integrations

### B1. Replace simulated inventory gateway with real room-service integration

Priority: `P0`
Status: `In progress`

Service:

- `services/bookings-service`

Main files:

- `services/bookings-service/src/Infrastructure/Adapters/Simulated/InventoryGatewaySimulated.cs`
- `services/bookings-service/src/Application/Abstractions/IInventoryGateway.cs`
- `services/bookings-service/src/Application/Handlers/ReserveInventoryCommandHandler.cs`
- `services/bookings-service/src/Infrastructure/DependencyInjection.cs`

Related service:

- `services/room-service`

What to do:

- normalize room identifier shape between services
- keep room reservation keyed by string booking reference
- validate the new real adapter against room-service reserve/release endpoints
- keep DI on the real adapter path

Done when:

- creating a booking reserves a real room hold
- cancelling/expiring a booking releases the hold

Current note:

- `bookings-service` now uses `long` room ids
- `room-service` reserve/release now accepts `booking_reference`
- a real HTTP inventory gateway is wired in bookings-service

---

### B2. Validate room availability before booking creation

Priority: `P0`
Status: `Started`

Service:

- `services/bookings-service`

Main files:

- `services/bookings-service/src/Application/Handlers/CreateBookingCommandHandler.cs`
- inventory adapter implementation from `B1`

What to do:

- check availability before creating/reserving booking
- return conflict or validation error for unavailable room

Done when:

- booking cannot be created for unavailable dates

Current note:

- `CreateBookingCommandHandler` now performs a real availability pre-check through the room-service-backed inventory gateway

---

### B3. Keep PMS simulated, but make it deterministic for demo

Priority: `P1`

Service:

- `services/bookings-service`

Main files:

- `services/bookings-service/src/Infrastructure/Adapters/Simulated/PmsGatewaySimulated.cs`

What to do:

- keep fake PMS for PoC
- reduce random behavior if it can make demos flaky
- keep confirmation timing predictable

Done when:

- successful payments reliably move toward confirmation during demo

---

## Workstream C - Payment Integration

Cross-cutting status:

- payment controller `try/catch` handling has been reduced by moving create-intent orchestration into a service and surfacing transport failures through middleware

### C1. Verify booking-to-payment amount flow

Priority: `P0`
Status: `Done`

Services:

- `services/bookings-service`
- `services/payment-service`

Main files:

- `services/bookings-service/src/Infrastructure/Adapters/PaymentGatewayGrpc.cs`
- `services/payment-service/src/PaymentService.API/Controllers/PaymentController.cs`

What to do:

- ensure booking sends real amount and currency
- ensure payment intent stores the expected values
- ensure booking total and payment total match

Done when:

- payment intent is created from real booking data without hardcoded values

Current note:

- `CreatePaymentCommandHandler` passes `booking.TotalPrice.Amount` to `IPaymentGateway.CreateIntentAsync` — real booking amount is used
- `POST /payment/intent` now returns the saga-pre-created intent when called with just `bookingId` (idempotent lookup)
- frontend and smoke test both send `{ bookingId }` only — controller looks up existing intent before validation

---

### C2. Verify webhook updates booking state correctly

Priority: `P0`
Status: `Done`

Services:

- `services/payment-service`
- `services/bookings-service`

Main files:

- `services/payment-service/src/PaymentService.API/Controllers/PaymentController.cs`
- booking consumers/state machine under `services/bookings-service/src/Infrastructure/Messaging/`

What to do:

- confirm event contract alignment for payment success/failure
- verify booking saga transitions on payment success
- verify booking failure/cancel path on payment failure

Done when:

- successful payment reliably moves booking to `Reserved` and then `Confirmed`

Current note:

- webhook controller fixed to accept flat `{ bookingId, status }` body (PoC test mode) in addition to nested `{ metadata: { bookingId }, status }` (real PSP shape)
- status comparison changed to case-insensitive (`"Succeeded"` and `"succeeded"` both accepted)
- `PaymentStatusChangedConsumer` correctly translates to `PaymentAuthorized` / `PaymentFailed` saga events
- saga transitions `AwaitingPayment → Reserved` on `PaymentAuthorized`, then `Reserved → Confirmed` after PMS stub

---

### C3. Add webhook HMAC validation

Priority: `P1`

Service:

- `services/payment-service`

Main files:

- `services/payment-service/src/PaymentService.API/Controllers/PaymentController.cs`
- configuration files under payment service

What to do:

- implement signature verification
- reject invalid webhook requests
- keep test-mode configuration easy to run locally

Done when:

- webhook endpoint no longer contains an unimplemented security TODO

---

## Workstream D - API Gateway

### D1. Route room APIs through gateway

Priority: `P0`
Status: `Done`

Services:

- `services/dev-gateway` (nginx)
- `services/room-service`

Main files:

- `services/dev-gateway/nginx/conf.d/gateway.conf`
- `services/dev-gateway/nginx/conf.d/upstreams.conf`

Done when:

- frontend can call room APIs through one gateway base URL

Current note:

- nginx dev-gateway at `:8080` routes `/api/rooms` → `room-service:8083`
- CORS headers added for `localhost:5173` (Vite dev server)

---

### D2. Route booking APIs through gateway

Priority: `P0`
Status: `Done`

Services:

- `services/dev-gateway` (nginx)
- `services/bookings-service`

Done when:

- frontend can complete booking flow through gateway only

Current note:

- nginx routes `/api/booking` → `bookingservice.api:80`
- WebSocket `/hubs/booking` proxied with Upgrade header for SignalR

---

### D3. Route pricing and payment APIs through gateway

Priority: `P0`
Status: `Done`

Services:

- `services/dev-gateway` (nginx)
- `services/pricing-service`
- `services/payment-service`

Done when:

- demo UI does not need to call internal service URLs directly

Current note:

- nginx routes `/api/pricing` → `pricing-service:5003`
- nginx routes `/payment` → `payment-service:80`; `/payment/webhook` skips auth

---

## Workstream E - Frontend

### E1. Initialize minimal guest frontend

Priority: `P0`
Status: `Done`

Service:

- `frontend/booking-frontend`

Done when:

- project runs locally and can render real pages

Current note:

- React 18 + Vite + TypeScript + react-router-dom v6
- `npm run dev` starts on `localhost:5173`; `VITE_GATEWAY_URL` env var controls API base URL

---

### E2. Build search page

Priority: `P0`
Status: `Done`

Service:

- `frontend/booking-frontend`

Done when:

- user can search and see rooms from real API data

Current note:

- `SearchPage.tsx`: date inputs, guest count, calls `GET /api/rooms/search`, shows room cards

---

### E3. Build room details page

Priority: `P0`
Status: `Done`

Service:

- `frontend/booking-frontend`

Done when:

- user can choose a room and continue

Current note:

- `RoomDetailsPage.tsx`: fetches room by id, shows amenities and images, passes `params` state to booking form

---

### E4. Build booking form page

Priority: `P0`
Status: `Done`

Service:

- `frontend/booking-frontend`

Done when:

- a booking is created from UI and returns a booking ID

Current note:

- `BookingFormPage.tsx`: hardcoded `POC_GUEST_ID` and `NIGHTLY_PRICE=120 EUR`; posts to `POST /api/booking`

---

### E5. Build payment page

Priority: `P0`
Status: `Done`

Service:

- `frontend/booking-frontend`

Done when:

- a user can complete test payment from UI

Current note:

- `PaymentPage.tsx`: polls for saga-pre-created intent with 8s retry backoff; simulates payment via `POST /payment/webhook { bookingId, status }`; polls booking status until Reserved/Confirmed

---

### E6. Build confirmation page

Priority: `P0`
Status: `Done`

Service:

- `frontend/booking-frontend`

Done when:

- successful payment leads to a visible confirmation screen

---

### E7. Add "My bookings" page

Priority: `P1`

Service:

- `frontend/booking-frontend`

What to do:

- list bookings for current user

Done when:

- demo can show post-booking retrieval flow

---

## Workstream F - Demo Validation

### F1. Create local PoC runbook

Priority: `P0`
Status: `Done`

Main file:

- `POC_RUNBOOK.md`

Done when:

- another developer can run the demo without tribal knowledge

Current note:

- `POC_RUNBOOK.md` documents first-run setup, demo happy path, curl checks, seed data, and troubleshooting table
- `docker-compose.poc.yml` is the all-in-one compose: infra + 5 databases + 5 services + nginx gateway

---

### F2. Run end-to-end smoke test

Priority: `P0`
Status: `Done (script exists; run to validate)`

Main file:

- `scripts/smoke-test.ps1`

What the script tests:

1. gateway health
2. search rooms
3. room availability
4. create booking
5. verify booking status = Created or AwaitingPayment
6. create payment intent (retries up to 10s for saga)
7. simulate payment webhook (Succeeded)
8. poll until booking reaches Reserved or Confirmed

Done when:

- all steps complete in one environment without manual DB fixes

---

### F3. Add one failure-path smoke check

Priority: `P1`

What to test:

- payment failure
- or booking cancellation

Done when:

- at least one non-happy-path behavior is demonstrated cleanly

---

## Cross-Cutting Risk

### R1. Booking concurrency collisions

Priority: `P1`
Status: `Started`

What exists now:

- optimistic concurrency protection has been started for booking persistence
- `confirm/cancel` now return conflict on detected concurrent write races
- room release now resolves all room ids from the booking before calling inventory

What still needs validation:

- parallel confirm vs cancel
- saga transition vs user-triggered cancel
- inventory/payment side effects under racing state transitions

### R2. Middleware-based API error handling

Priority: `P1`
Status: `Started`

What exists now:

- booking, pricing, and payment APIs now use dedicated exception-handling middleware
- controller-local `try/catch` blocks have been removed from current controllers

What still needs validation:

- ensure response shapes stay consistent across services
- decide whether to standardize all validation failures on `ProblemDetails`
- add automated API tests for conflict and provider-unavailable responses

---

## Recommended Execution Order

### Phase 1 - Backend unblockers

1. `A1` implement confirm
2. `A2` implement cancel
3. `A3` booking validation
4. `B1` real inventory integration
5. `B2` availability check before booking
6. `C1` verify amount flow
7. `C2` verify webhook-to-booking status flow

### Phase 2 - Entry point

1. `D1` room routes
2. `D2` booking routes
3. `D3` pricing/payment routes

### Phase 3 - Demo UI

1. `E1` initialize app
2. `E2` search page
3. `E3` room details
4. `E4` booking form
5. `E5` payment page
6. `E6` confirmation page

### Phase 4 - Demo hardening

1. `F1` runbook
2. `F2` smoke test
3. `B3` deterministic simulated PMS
4. `C3` webhook HMAC
5. `E7` my bookings
6. `F3` failure-path smoke test

---

## Suggested Ownership Split

If work is parallelized, a reasonable split is:

- Backend engineer 1:
  - `A1`, `A2`, `A3`, `A4`

- Backend engineer 2:
  - `B1`, `B2`, `B3`, `C1`, `C2`, `C3`

- Platform engineer:
  - `D1`, `D2`, `D3`, `F1`

- Frontend engineer:
  - `E1` through `E7`

- Shared validation:
  - `F2`, `F3`

---

## Definition of Done

This task list is complete enough for PoC when:

- all `P0` tasks are done
- the happy path runs end-to-end from UI
- the flow is documented and repeatable locally
