using BookingService.Application.Commands;
using BookingService.Application.Handlers;
using BookingService.Domain.Aggregates.Booking;

using MediatR;

using Moq;

using NUnit.Framework;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BookingService.Tests;

[TestFixture]
public class ConfirmBookingCommandHandlerTests
{
    [Test]
    public async Task Handle_ShouldRequestConfirmedStatusTransition()
    {
        var bookingId = Guid.NewGuid();
        var sender = new Mock<ISender>();
        var handler = new ConfirmBookingCommandHandler(sender.Object);

        await handler.Handle(new ConfirmBookingCommand(bookingId), CancellationToken.None);

        sender.Verify(
            x => x.Send(
                It.Is<SetBookingStatusCommand>(c => c.BookingId == bookingId && c.Status == BookingStatus.Confirmed),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
