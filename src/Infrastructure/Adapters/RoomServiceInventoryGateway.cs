using Bookings.Common.Exceptions;

using BookingService.Application.Abstractions;

using Microsoft.Extensions.Logging;

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace BookingService.Infrastructure.Adapters;

public class RoomServiceInventoryGateway : IInventoryGateway
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RoomServiceInventoryGateway> _logger;

    public RoomServiceInventoryGateway(
        HttpClient httpClient,
        ILogger<RoomServiceInventoryGateway> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task CheckAvailabilityAsync(long roomId, DateOnly checkIn, DateOnly checkOut, CancellationToken ct)
    {
        // RFC3339 with Z required for Go's time.Time query binding
        var checkInStr = $"{checkIn:yyyy-MM-dd}T00:00:00Z";
        var checkOutStr = $"{checkOut:yyyy-MM-dd}T00:00:00Z";

        using var response = await _httpClient.GetAsync(
            $"/api/rooms/{roomId}/availability?checkIn={checkInStr}&checkOut={checkOutStr}",
            ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new InvalidOperationException($"Room {roomId} not found");

        if (!response.IsSuccessStatusCode)
        {
            var error = await ReadErrorAsync(response, ct);
            throw new HttpRequestException(
                $"Availability check failed for room {roomId}: {error}",
                null,
                response.StatusCode);
        }

        var payload = await response.Content.ReadFromJsonAsync<AvailabilityResponse>(cancellationToken: ct)
            ?? throw new HttpRequestException($"Availability check returned an empty payload for room {roomId}.");

        if (!payload.IsAvailable)
            throw new BusinessRuleException($"Room {roomId} is not available for the selected dates.");
    }

    public async Task ReserveAsync(
        Guid bookingId,
        long roomId,
        DateOnly checkIn,
        DateOnly checkOut,
        CancellationToken ct)
    {
        var request = new ReserveRoomRequest(
            $"{checkIn:yyyy-MM-dd}T00:00:00Z",
            $"{checkOut:yyyy-MM-dd}T00:00:00Z",
            bookingId.ToString());

        using var response = await _httpClient.PostAsJsonAsync(
            $"/api/rooms/{roomId}/reserve",
            request,
            ct);

        if (response.IsSuccessStatusCode)
            return;

        var error = await ReadErrorAsync(response, ct);
        _logger.LogWarning(
            "Failed to reserve room {RoomId} for booking {BookingId}. Status: {StatusCode}. Error: {Error}",
            roomId,
            bookingId,
            (int)response.StatusCode,
            error);

        if (response.StatusCode == HttpStatusCode.Conflict)
            throw new BusinessRuleException(error);

        throw new HttpRequestException(
            $"Room reservation failed for room {roomId}: {error}",
            null,
            response.StatusCode);
    }

    public async Task ReleaseAsync(Guid bookingId, long roomId, CancellationToken ct)
    {
        var request = new ReleaseRoomRequest(bookingId.ToString());

        using var response = await _httpClient.PostAsJsonAsync(
            $"/api/rooms/{roomId}/release",
            request,
            ct);

        if (response.IsSuccessStatusCode)
            return;

        var error = await ReadErrorAsync(response, ct);
        _logger.LogWarning(
            "Failed to release room {RoomId} for booking {BookingId}. Status: {StatusCode}. Error: {Error}",
            roomId,
            bookingId,
            (int)response.StatusCode,
            error);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return;

        throw new HttpRequestException(
            $"Room release failed for room {roomId}: {error}",
            null,
            response.StatusCode);
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var body = await response.Content.ReadAsStringAsync(ct);
        return string.IsNullOrWhiteSpace(body)
            ? response.ReasonPhrase ?? "Unknown room-service error"
            : body;
    }

    // Dates sent as RFC3339 strings so Go's time.Time form/json binding parses correctly
    private sealed record ReserveRoomRequest(
        [property: JsonPropertyName("check_in")] string CheckIn,
        [property: JsonPropertyName("check_out")] string CheckOut,
        [property: JsonPropertyName("booking_reference")] string BookingReference);

    private sealed record ReleaseRoomRequest(
        [property: JsonPropertyName("booking_reference")] string BookingReference);

    // Go returns snake_case JSON; JsonPropertyName ensures correct deserialization
    private sealed record AvailabilityResponse(
        [property: JsonPropertyName("room_id")] long RoomId,
        [property: JsonPropertyName("is_available")] bool IsAvailable,
        [property: JsonPropertyName("check_in")] string CheckIn,
        [property: JsonPropertyName("check_out")] string CheckOut);
}
