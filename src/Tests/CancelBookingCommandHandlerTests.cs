using Bookings.Common.ValueObjects;

using BookingService.Application.Commands;
using BookingService.Application.Handlers;
using BookingService.Application.Interfaces;
using BookingService.Domain.Aggregates.Booking;

using MediatR;

using Moq;

using NUnit.Framework;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BookingService.Tests;

[TestFixture]
public class CancelBookingCommandHandlerTests
{
    [Test]
    public async Task Handle_WhenReserved_ShouldCancelReleaseInventoryAndRefund()
    {
        var booking = CreateReservedBooking();
        var repository = new Mock<IBookingRepository>();
        repository.Setup(x => x.GetAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.SetupGet(x => x.Bookings).Returns(repository.Object);

        var sender = new Mock<ISender>();

        var handler = new CancelBookingCommandHandler(unitOfWork.Object, sender.Object);

        await handler.Handle(new CancelBookingCommand(booking.Id, "Cancelled by test"), CancellationToken.None);

        sender.Verify(
            x => x.Send(
                It.Is<SetBookingStatusCommand>(c => c.BookingId == booking.Id && c.Status == BookingStatus.Cancelled),
                It.IsAny<CancellationToken>()),
            Times.Once);
        sender.Verify(
            x => x.Send(
                It.Is<ReleaseInventoryCommand>(c => c.BookingId == booking.Id),
                It.IsAny<CancellationToken>()),
            Times.Once);
        sender.Verify(
            x => x.Send(
                It.Is<RefundPaymentCommand>(c => c.BookingId == booking.Id),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Handle_WhenAwaitingPayment_ShouldCancelAndReleaseWithoutRefund()
    {
        var booking = CreateAwaitingPaymentBooking();
        var repository = new Mock<IBookingRepository>();
        repository.Setup(x => x.GetAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.SetupGet(x => x.Bookings).Returns(repository.Object);

        var sender = new Mock<ISender>();

        var handler = new CancelBookingCommandHandler(unitOfWork.Object, sender.Object);

        await handler.Handle(new CancelBookingCommand(booking.Id, "Cancelled by test"), CancellationToken.None);

        sender.Verify(
            x => x.Send(
                It.Is<SetBookingStatusCommand>(c => c.BookingId == booking.Id && c.Status == BookingStatus.Cancelled),
                It.IsAny<CancellationToken>()),
            Times.Once);
        sender.Verify(
            x => x.Send(
                It.Is<ReleaseInventoryCommand>(c => c.BookingId == booking.Id),
                It.IsAny<CancellationToken>()),
            Times.Once);
        sender.Verify(
            x => x.Send(It.IsAny<RefundPaymentCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static Booking CreateReservedBooking()
    {
        var booking = Booking.Create(Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow.Date), DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(2)));
        booking.AddLineItem(101, 2, 0, 2, new Money(100m, "EUR"));
        booking.MarkAwaitingPayment();
        booking.MarkReserved();
        return booking;
    }

    private static Booking CreateAwaitingPaymentBooking()
    {
        var booking = Booking.Create(Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow.Date), DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(2)));
        booking.AddLineItem(101, 2, 0, 2, new Money(100m, "EUR"));
        booking.MarkAwaitingPayment();
        return booking;
    }
}
