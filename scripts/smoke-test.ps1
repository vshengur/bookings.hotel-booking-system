# Hotel Booking PoC — End-to-end smoke test
# Usage: .\scripts\smoke-test.ps1 [-GatewayUrl http://localhost:8080]

param(
    [string]$GatewayUrl = "http://localhost:8080"
)

$ErrorActionPreference = "Stop"
$bookingId = [System.Guid]::NewGuid().ToString()
$guestId   = "00000000-0000-0000-0000-000000000001"
$checkIn   = (Get-Date).AddDays(7).ToString("yyyy-MM-dd")
$checkOut  = (Get-Date).AddDays(10).ToString("yyyy-MM-dd")
$passed    = 0
$failed    = 0

function Step([string]$name, [scriptblock]$action) {
    Write-Host "`n[$name]" -ForegroundColor Cyan
    try {
        & $action
        Write-Host "  PASS" -ForegroundColor Green
        $script:passed++
    } catch {
        Write-Host "  FAIL: $_" -ForegroundColor Red
        $script:failed++
    }
}

function Invoke([string]$method, [string]$path, $body = $null) {
    $uri = "$GatewayUrl$path"
    $args = @{ Method = $method; Uri = $uri; ContentType = "application/json" }
    if ($body) { $args.Body = ($body | ConvertTo-Json -Depth 10) }
    Invoke-RestMethod @args
}

# ── Step 1: Gateway health ─────────────────────────────────────────────────
Step "1. Gateway health" {
    $r = Invoke GET "/healthz"
    if ($r -notmatch "OK") { throw "Unexpected health response: $r" }
}

# ── Step 2: Search rooms ───────────────────────────────────────────────────
$roomId = 0
Step "2. Search rooms" {
    $r = Invoke GET "/api/rooms/search?checkIn=${checkIn}T00:00:00Z&checkOut=${checkOut}T00:00:00Z&adults=2&children=0"
    if ($r.rooms.Count -eq 0) { throw "No rooms returned — add seed data first (see POC_RUNBOOK.md §6)" }
    $script:roomId = $r.rooms[0].id
    Write-Host "  Found room id=$($script:roomId)"
}

# ── Step 3: Room availability ──────────────────────────────────────────────
Step "3. Room availability" {
    $r = Invoke GET "/api/rooms/$($script:roomId)/availability?checkIn=${checkIn}T00:00:00Z&checkOut=${checkOut}T00:00:00Z"
    if (-not $r.is_available) { throw "Room $($script:roomId) is not available for test dates" }
}

# ── Step 4: Create booking ─────────────────────────────────────────────────
Step "4. Create booking" {
    $body = @{
        bookingId = $bookingId
        guestId   = $guestId
        checkIn   = $checkIn
        checkOut  = $checkOut
        items     = @(@{
            roomId       = $script:roomId
            adults       = 2
            children     = 0
            nights       = 3
            pricePerNight = @{ amount = 120; currency = "EUR" }
        })
    }
    $r = Invoke POST "/api/booking" $body
    Write-Host "  bookingId=$($r.bookingId)"
}

# ── Step 5: Verify booking created ────────────────────────────────────────
Step "5. Booking status = Created or AwaitingPayment" {
    Start-Sleep -Seconds 2
    $r = Invoke GET "/api/booking/$bookingId"
    if ($r.status -notin @("Created", "AwaitingPayment")) {
        throw "Unexpected status after create: $($r.status)"
    }
    Write-Host "  status=$($r.status)"
}

# ── Step 6: Create payment intent ─────────────────────────────────────────
Step "6. Create payment intent" {
    $r = Invoke POST "/payment/intent" @{ bookingId = $bookingId }
    Write-Host "  intentId=$($r.intentId)"
}

# ── Step 7: Simulate payment success ──────────────────────────────────────
Step "7. Simulate payment webhook (Succeeded)" {
    Invoke POST "/payment/webhook" @{ bookingId = $bookingId; status = "Succeeded" } | Out-Null
}

# ── Step 8: Wait and verify booking advanced ───────────────────────────────
Step "8. Booking advances to Reserved or Confirmed" {
    $final = @("Reserved", "Confirmed", "Failed", "Cancelled")
    for ($i = 0; $i -lt 15; $i++) {
        Start-Sleep -Seconds 2
        $r = Invoke GET "/api/booking/$bookingId"
        Write-Host "  poll $($i+1)/15: status=$($r.status)"
        if ($r.status -in $final) {
            if ($r.status -in @("Failed", "Cancelled")) {
                throw "Booking ended in failure state: $($r.status)"
            }
            Write-Host "  Final status: $($r.status)"
            return
        }
    }
    throw "Booking did not advance after 30s. Last status: $($r.status)"
}

# ── Summary ────────────────────────────────────────────────────────────────
Write-Host "`n══════════════════════════════" -ForegroundColor White
Write-Host " Smoke test complete" -ForegroundColor White
Write-Host " PASSED: $passed   FAILED: $failed" -ForegroundColor $(if ($failed -eq 0) { "Green" } else { "Red" })
Write-Host "══════════════════════════════" -ForegroundColor White

if ($failed -gt 0) { exit 1 }
