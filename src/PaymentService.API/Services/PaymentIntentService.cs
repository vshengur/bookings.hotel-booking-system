using MassTransit;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using PaymentService.Application.Builders;
using PaymentService.Application.Configuration;
using PaymentService.Application.DTOs;
using PaymentService.Application.Helpers;
using PaymentService.Application.Messages;
using PaymentService.Infrastructure.Persistence;
using PaymentService.Infrastructure.PSPClient;

namespace PaymentService.API.Services;

public interface IPaymentIntentService
{
    Task<CreatePaymentIntentResult> CreateIntentAsync(
        CreatePaymentIntentRequest request,
        CancellationToken cancellationToken);
}

public record CreatePaymentIntentResult(
    CreatePaymentIntentResponse Response,
    bool ReusedExistingIntent);

public class PaymentIntentService : IPaymentIntentService
{
    private readonly PaymentServiceProviderClient _psp;
    private readonly PaymentDbContext _db;
    private readonly IPublishEndpoint _bus;
    private readonly PaymentSettings _paymentSettings;
    private readonly ILogger<PaymentIntentService> _logger;

    public PaymentIntentService(
        PaymentServiceProviderClient psp,
        PaymentDbContext db,
        IPublishEndpoint bus,
        IOptions<PaymentSettings> paymentOptions,
        ILogger<PaymentIntentService> logger)
    {
        _psp = psp;
        _db = db;
        _bus = bus;
        _paymentSettings = paymentOptions.Value;
        _logger = logger;
    }

    public async Task<CreatePaymentIntentResult> CreateIntentAsync(
        CreatePaymentIntentRequest request,
        CancellationToken cancellationToken)
    {
        var intent = new PaymentIntent
        {
            Id = Guid.NewGuid(),
            BookingId = request.BookingId,
            Amount = request.Amount ?? 0,
            Currency = (request.Currency ?? "EUR").ToUpperInvariant(),
            Status = "pending",
            CreatedAt = DateTime.UtcNow,
        };

        _db.PaymentIntents.Add(intent);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            _db.Entry(intent).State = EntityState.Detached;

            var existing = await _db.PaymentIntents
                .AsNoTracking()
                .FirstAsync(item => item.BookingId == request.BookingId, cancellationToken);

            _logger.LogInformation(
                "PaymentIntent already exists for booking {BookingId}, returning existing {IntentId}",
                request.BookingId, existing.Id);

            return new CreatePaymentIntentResult(
                new CreatePaymentIntentResponse(
                    existing.Id,
                    existing.BookingId,
                    existing.Amount,
                    existing.Currency,
                    existing.Status),
                true);
        }

        // Call real PSP; on failure (PoC: PSP may not be configured), fall back to stub mode.
        string pspJson = string.Empty;
        try
        {
            var money = new Bookings.Common.ValueObjects.Money(
                (request.Amount ?? 0) / 100m, request.Currency ?? "EUR");

            var payload = new PaymentPayloadBuilder()
                .OrderId(request.BookingId)
                .Amount(money)
                .ReturnUrl(_paymentSettings.ReturnUrl)
                .CancelUrl(_paymentSettings.CancelUrl)
                .Build();

            pspJson = await _psp.CreateIntentAsync(payload, cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(ex,
                "PSP unavailable for booking {BookingId} — using stub PoC mode for intent {IntentId}",
                request.BookingId, intent.Id);
            // PoC stub: leave pspJson empty, intent is created in pending state.
            // Payment will be confirmed via webhook simulation.
        }

        intent.ProviderRef = string.IsNullOrEmpty(pspJson)
            ? $"poc-stub-{intent.Id:N}"
            : PspResponseParser.ExtractProviderRef(pspJson);
        intent.Status = "created";
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "PaymentIntent {IntentId} created for booking {BookingId}, amount {Amount} {Currency}",
            intent.Id, intent.BookingId, intent.Amount, intent.Currency);

        await _bus.Publish(new PaymentIntentCreated(
            intent.BookingId, intent.Id, intent.Amount,
            intent.Currency, pspJson), cancellationToken);

        return new CreatePaymentIntentResult(
            new CreatePaymentIntentResponse(
                intent.Id,
                intent.BookingId,
                intent.Amount,
                intent.Currency,
                intent.Status),
            false);
    }

}
