using Bookings.Common.ValueObjects;

using MassTransit;

using System;

namespace BookingService.Infrastructure.Messaging.MassTransit;

public class BookingState : SagaStateMachineInstance, ISagaVersion
{
    public required Guid CorrelationId { get; set; }
    public int Version { get; set; }

    public DateTime Created { get; set; }

    public string? CurrentState { get; set; }

    public string? PaymentRef { get; set; }
    public DateTime? PaymentRefUpdated { get; set; }

    public string? PmsNumber { get; set; }
    public DateTime? PmsNumberUpdated { get; set; }

    public Guid? PaymentTimeoutTokenId { get; set; }
}