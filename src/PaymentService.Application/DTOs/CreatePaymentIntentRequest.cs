using System;

namespace PaymentService.Application.DTOs;

public sealed record CreatePaymentIntentRequest(
    Guid BookingId,
    long? Amount = null,
    string? Currency = null);
