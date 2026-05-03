using Bookings.Common.Exceptions;

using BookingService.Application.Abstractions;

using Microsoft.Extensions.Logging;

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
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

    public async Task ReserveAsync(
        Guid bookingId,
        long roomId,
        DateOnly checkIn,
        DateOnly checkOut,
        CancellationToken ct)
    {
        var request = new ReserveRoomRequest(
            checkIn.ToDateTime(TimeOnly.MinValue),
            checkOut.ToDateTime(TimeOnly.MinValue),
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

    private sealed record ReserveRoomRequest(
        DateTime CheckIn,
        DateTime CheckOut,
        string BookingReference);

    private sealed record ReleaseRoomRequest(string BookingReference);
}
