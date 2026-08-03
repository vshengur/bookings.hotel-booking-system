using BookingService.Application.Abstractions;
using BookingService.Application.Interfaces;

using Microsoft.Extensions.Logging;

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace BookingService.Infrastructure.Adapters;

public class PaymentGatewayHttp : IPaymentGateway
{
    private readonly HttpClient _http;
    private readonly IBookingRepository _repo;
    private readonly ILogger<PaymentGatewayHttp> _logger;

    public PaymentGatewayHttp(
        HttpClient http,
        IBookingRepository repo,
        ILogger<PaymentGatewayHttp> logger)
    {
        _http = http;
        _repo = repo;
        _logger = logger;
    }

    public async Task CreateIntentAsync(Guid bookingId, decimal amount, CancellationToken ct)
    {
        var booking = await _repo.GetAsync(bookingId, ct)
            ?? throw new InvalidOperationException($"Booking {bookingId} not found");

        var totalAmount = booking.TotalPrice.Amount;
        var currency = booking.TotalPrice.Currency;

        _logger.LogInformation(
            "Creating payment intent for booking {BookingId} amount {Amount} {Currency}",
            bookingId, totalAmount, currency);

        var body = new CreateIntentBody(bookingId, (long)(totalAmount * 100), currency);
        using var response = await _http.PostAsJsonAsync("/payment/intent", body, ct);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException(
                $"Payment intent creation failed ({(int)response.StatusCode}): {err}",
                null, response.StatusCode);
        }

        _logger.LogInformation("Payment intent created for booking {BookingId}", bookingId);
    }

    public async Task RefundAsync(Guid bookingId, CancellationToken ct)
    {
        _logger.LogInformation("Processing refund for booking {BookingId}", bookingId);
        using var response = await _http.PostAsJsonAsync($"/payment/refund/{bookingId}", new { }, ct);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(ct);
            _logger.LogWarning("Refund failed for booking {BookingId}: {Error}", bookingId, err);
        }
    }

    private sealed record CreateIntentBody(
        [property: JsonPropertyName("bookingId")] Guid BookingId,
        [property: JsonPropertyName("amount")]    long Amount,
        [property: JsonPropertyName("currency")]  string Currency);
}
