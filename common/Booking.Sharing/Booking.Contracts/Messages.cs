using Bookings.Common.ValueObjects;

namespace Bookings.Contracts;

public interface ICorrelated
{
    Guid BookingId { get; }
}

public enum BookingExternalStatus
{
    Pending,
    AwaitingPayment,
    Reserved,
    Confirmed,
    Cancelled,
    Failed,
    Expired
}

public record CreateBooking(Guid BookingId) : ICorrelated;

public record BookingCreated(Guid BookingId, Money TotalPrice) : ICorrelated;

public record PaymentAuthorized(Guid BookingId, string PaymentProviderRef) : ICorrelated;

public record PaymentFailed(Guid BookingId, string Error) : ICorrelated;

public record ConfirmInPms(Guid BookingId) : ICorrelated;

public record PmsConfirmed(Guid BookingId, string PmsNumber) : ICorrelated;

public record CancelBooking(Guid BookingId, string Reason) : ICorrelated;

public record BookingCancelled(Guid BookingId, string Reason) : ICorrelated;