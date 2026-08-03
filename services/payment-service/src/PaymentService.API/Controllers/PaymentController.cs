using FluentValidation;

using MassTransit;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using PaymentService.Application.DTOs;
using PaymentService.Application.Messages;
using PaymentService.API.Services;
using PaymentService.Infrastructure.Persistence;
using PaymentService.Infrastructure.PSPClient;

using System.Text.Json;

namespace PaymentService.API.Controllers;

[ApiController]
[Route("payment")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentIntentService _paymentIntentService;
    private readonly PaymentDbContext _db;
    private readonly IPublishEndpoint _bus;
    private readonly PaymentServiceProviderClient _psp;
    private readonly IValidator<CreatePaymentIntentRequest> _validator;

    public PaymentController(
        IPaymentIntentService paymentIntentService,
        PaymentDbContext db,
        IPublishEndpoint bus,
        PaymentServiceProviderClient psp,
        IValidator<CreatePaymentIntentRequest> validator)
    {
        _paymentIntentService = paymentIntentService;
        _db = db;
        _bus = bus;
        _psp = psp;
        _validator = validator;
    }

    [HttpPost("intent")]
    public async Task<IActionResult> CreateIntent(
        [FromBody] CreatePaymentIntentRequest request,
        CancellationToken ct)
    {
        // The booking saga pre-creates the intent via gRPC before the user reaches the payment page.
        // When the frontend calls this endpoint with just { bookingId }, return the existing intent.
        var existing = await _db.PaymentIntents
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.BookingId == request.BookingId, ct);

        if (existing is not null)
            return Ok(new CreatePaymentIntentResponse(
                existing.Id, existing.BookingId, existing.Amount, existing.Currency, existing.Status));

        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

        var result = await _paymentIntentService.CreateIntentAsync(request, ct);
        return result.ReusedExistingIntent
            ? Ok(result.Response)
            : Accepted(result.Response);
    }

    [HttpPost("refund/{bookingId:guid}")]
    public async Task<IActionResult> Refund(Guid bookingId, CancellationToken ct)
    {
        var intent = await _db.PaymentIntents
            .FirstOrDefaultAsync(i => i.BookingId == bookingId, ct);

        if (intent is null)
            return NotFound();

        var ok = await _psp.RefundAsync(intent.Id, intent.Amount, ct);
        if (ok)
        {
            intent.Status = "refunded";
            await _db.SaveChangesAsync(ct);

            await _bus.Publish(new PaymentStatusChanged(bookingId, "Refunded"), ct);
        }

        return Ok(ok);
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook(
        [FromHeader(Name = "X-Signature")] string? sig,
        [FromBody] JsonElement body,
        CancellationToken ct)
    {
        // TODO(PAY-002): validate HMAC signature
        var statusRaw = body.GetProperty("status").GetString() ?? string.Empty;
        var succeeded = statusRaw.Equals("succeeded", StringComparison.OrdinalIgnoreCase)
                     || statusRaw.Equals("Succeeded", StringComparison.OrdinalIgnoreCase);

        // Support both flat { bookingId, status } (PoC test mode) and
        // nested { status, metadata: { bookingId } } (real PSP webhook shape)
        Guid bookingId;
        if (body.TryGetProperty("bookingId", out var flat))
            bookingId = Guid.Parse(flat.GetString()!);
        else
            bookingId = Guid.Parse(body.GetProperty("metadata").GetProperty("bookingId").GetString()!);

        var status = succeeded ? "Succeeded" : "Failed";
        await _bus.Publish(new PaymentStatusChanged(bookingId, status), ct);

        return Ok();
    }
}
