using System;
using System.Linq;

using FluentValidation;

using Microsoft.Extensions.Options;

using PaymentService.Application.Configuration;
using PaymentService.Application.DTOs;

namespace PaymentService.Application.Validators;

public sealed class CreatePaymentIntentRequestValidator : AbstractValidator<CreatePaymentIntentRequest>
{
    public CreatePaymentIntentRequestValidator(IOptions<PaymentSettings> paymentOptions)
    {
        var allowed = paymentOptions.Value.AllowedCurrencies;

        RuleFor(x => x.BookingId)
            .NotEmpty()
            .WithMessage("BookingId is required.");

        RuleFor(x => x.Amount)
            .NotNull().WithMessage("Amount is required.")
            .GreaterThan(0).When(x => x.Amount.HasValue)
            .WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required.")
            .Must(c => c == null || allowed.Contains(c, StringComparer.OrdinalIgnoreCase))
            .WithMessage(x => $"Currency '{x.Currency}' is not supported. Allowed: {string.Join(", ", allowed)}.")
            .When(x => x.Currency != null);
    }
}
