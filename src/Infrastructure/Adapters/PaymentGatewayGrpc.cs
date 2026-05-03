using BookingService.Application.Abstractions;
using BookingService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using PaymentService.Contracts.Grpc.V1;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BookingService.Infrastructure.Adapters;

public class PaymentGatewayGrpc : IPaymentGateway
{
    private readonly PaymentService.Contracts.Grpc.V1.PaymentService.PaymentServiceClient _client;
    private readonly IBookingRepository _bookingRepository;
    private readonly ILogger<PaymentGatewayGrpc> _logger;

    public PaymentGatewayGrpc(
        PaymentService.Contracts.Grpc.V1.PaymentService.PaymentServiceClient client,
        IBookingRepository bookingRepository,
        ILogger<PaymentGatewayGrpc> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task CreateIntentAsync(Guid bookingId, decimal amount, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Creating payment intent for booking {BookingId} with amount {Amount}", bookingId, amount);

            // Получаем booking для дополнительной информации
            var booking = await _bookingRepository.GetAsync(bookingId, ct)
                ?? throw new InvalidOperationException($"Booking {bookingId} not found");

            // Берем первый room_id из items
            var roomId = booking.Items.FirstOrDefault()?.RoomId.ToString()
                ?? throw new InvalidOperationException($"No room found for booking {bookingId}");

            var request = new CreateIntentRequest
            {
                BookingId = bookingId.ToString(),
                RoomId = roomId,
                CustomerEmail = $"{booking.GuestId}@guest.local", // TODO: получать реальный email из guest service
                Amount = new Money
                {
                    Currency = booking.TotalPrice.Currency,
                    AmountMinor = (long)(amount * 100)
                }
            };

            var response = await _client.CreateIntentAsync(request, cancellationToken: ct);

            _logger.LogInformation(
                "Payment intent created successfully. IntentId: {IntentId}, RedirectUrl: {RedirectUrl}",
                response.IntentId,
                response.PspRedirectUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create payment intent for booking {BookingId}", bookingId);
            throw;
        }
    }

    public async Task RefundAsync(Guid bookingId, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Processing refund for booking {BookingId}", bookingId);

            var request = new RefundRequest
            {
                BookingId = bookingId.ToString(),
                // TODO: если нужно указать сумму возврата, добавить параметр amount в интерфейс
                AmountMinor = 0 // 0 означает полный возврат
            };

            var response = await _client.RefundAsync(request, cancellationToken: ct);

            if (!response.Success)
            {
                _logger.LogWarning("Refund failed for booking {BookingId}. RefundId: {RefundId}", bookingId, response.RefundId);
                throw new InvalidOperationException($"Refund failed for booking {bookingId}. RefundId: {response.RefundId}");
            }

            _logger.LogInformation("Refund processed successfully for booking {BookingId}. RefundId: {RefundId}", bookingId, response.RefundId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process refund for booking {BookingId}", bookingId);
            throw;
        }
    }
}

