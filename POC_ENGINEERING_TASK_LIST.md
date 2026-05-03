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

Done when:

- invalid requests fail early with usable API errors

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
Status: `Not started`

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

---

### C2. Verify webhook updates booking state correctly

Priority: `P0`

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

Services:

- `services/api-gateway` or `services/api-gateway-golang`
- `services/room-service`

Main files:

- C# gateway config and provider files
- or Go gateway route files such as `internal/routes/routes.go`

What to do:

- expose room search/details/availability via gateway

Done when:

- frontend can call room APIs through one gateway base URL

---

### D2. Route booking APIs through gateway

Priority: `P0`

Services:

- gateway service
- `services/bookings-service`

What to do:

- add create/get/confirm/cancel booking routes
- ensure auth expectations are clear

Done when:

- frontend can complete booking flow through gateway only

---

### D3. Route pricing and payment APIs through gateway

Priority: `P0`

Services:

- gateway service
- `services/pricing-service`
- `services/payment-service`

What to do:

- add route definitions for pricing and payment
- normalize path conventions where possible

Done when:

- demo UI does not need to call internal service URLs directly

---

## Workstream E - Frontend

### E1. Initialize minimal guest frontend

Priority: `P0`

Service:

- `frontend/booking-frontend`

What to do:

- replace placeholder repo contents with actual app scaffold
- choose a minimal stack and keep it small

Recommended initial scope:

- one app
- basic router/pages
- simple API client

Done when:

- project runs locally and can render real pages

---

### E2. Build search page

Priority: `P0`

Service:

- `frontend/booking-frontend`

Backend dependencies:

- gateway
- room-service

What to do:

- date inputs
- guest count
- search submission
- results list

Done when:

- user can search and see rooms from real API data

---

### E3. Build room details page

Priority: `P0`

Service:

- `frontend/booking-frontend`

Backend dependencies:

- room-service
- pricing-service if used directly

What to do:

- display room information
- show price summary
- allow proceed to booking

Done when:

- user can choose a room and continue

---

### E4. Build booking form page

Priority: `P0`

Service:

- `frontend/booking-frontend`

Backend dependencies:

- bookings-service

What to do:

- capture guest details
- create booking
- handle booking API errors

Done when:

- a booking is created from UI and returns a booking ID

---

### E5. Build payment page

Priority: `P0`

Service:

- `frontend/booking-frontend`

Backend dependencies:

- payment-service

What to do:

- request payment intent
- complete payment in test mode
- show progress and failure states

Done when:

- a user can complete test payment from UI

---

### E6. Build confirmation page

Priority: `P0`

Service:

- `frontend/booking-frontend`

Backend dependencies:

- bookings-service

What to do:

- show final booking status
- display booking identifier and summary

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

Main file candidates:

- root README or dedicated PoC setup doc

What to do:

- document services to start
- document env requirements
- document demo credentials and test card flow

Done when:

- another developer can run the demo without tribal knowledge

---

### F2. Run end-to-end smoke test

Priority: `P0`

What to test:

1. login
2. search room
3. open room
4. create booking
5. reserve room
6. create payment intent
7. complete test payment
8. see final confirmed state

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
