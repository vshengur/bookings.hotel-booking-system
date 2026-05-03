using Bookings.Common.Exceptions;
using Bookings.Common.ValueObjects;

using BookingService.Application.Abstractions;
using BookingService.Application.Commands;
using BookingService.Application.Events;
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
public class CreateBookingCommandHandlerTests
{
    [Test]
    public async Task Handle_WhenRoomIsAvailable_ShouldPersistBookingAndPublishEvent()
    {
        var inventoryGateway = new Mock<IInventoryGateway>();
        var repository = new Mock<IBookingRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.SetupGet(x => x.Bookings).Returns(repository.Object);

        var publisher = new Mock<IPublisher>();

        var handler = new CreateBookingCommandHandler(
            inventoryGateway.Object,
            unitOfWork.Object,
            publisher.Object);
        var requestHandler = (IRequestHandler<CreateBookingCommand>)handler;

        var bookingId = Guid.NewGuid();
        var command = new CreateBookingCommand(
            bookingId,
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1)),
            DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(3)),
            [
                new BookingLineItem(bookingId, 101, 2, 0, 2, new Money(100m, "EUR"))
            ],
            null);

        await requestHandler.Handle(command, CancellationToken.None);

        inventoryGateway.Verify(
            x => x.CheckAvailabilityAsync(101, command.CheckIn, command.CheckOut, It.IsAny<CancellationToken>()),
            Times.Once);
        repository.Verify(x => x.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        publisher.Verify(
            x => x.Publish(
                It.Is<BookingInitiated>(e => e.BookingId == bookingId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public void Handle_WhenRoomIsUnavailable_ShouldStopBeforePersisting()
    {
        var inventoryGateway = new Mock<IInventoryGateway>();
        inventoryGateway
            .Setup(x => x.CheckAvailabilityAsync(It.IsAny<long>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleException("Room is not available"));

        var repository = new Mock<IBookingRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.SetupGet(x => x.Bookings).Returns(repository.Object);

        var publisher = new Mock<IPublisher>();

        var handler = new CreateBookingCommandHandler(
            inventoryGateway.Object,
            unitOfWork.Object,
            publisher.Object);
        var requestHandler = (IRequestHandler<CreateBookingCommand>)handler;

        var bookingId = Guid.NewGuid();
        var command = new CreateBookingCommand(
            bookingId,
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1)),
            DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(3)),
            [
                new BookingLineItem(bookingId, 101, 2, 0, 2, new Money(100m, "EUR"))
            ],
            null);

        Assert.ThrowsAsync<BusinessRuleException>(async () =>
            await requestHandler.Handle(command, CancellationToken.None));

        repository.Verify(x => x.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        publisher.Verify(
            x => x.Publish(It.IsAny<BookingInitiated>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public void Handle_WhenCheckInIsInThePast_ShouldFailBeforeAvailabilityCheck()
    {
        var inventoryGateway = new Mock<IInventoryGateway>();
        var repository = new Mock<IBookingRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.SetupGet(x => x.Bookings).Returns(repository.Object);
        var publisher = new Mock<IPublisher>();

        var handler = new CreateBookingCommandHandler(
            inventoryGateway.Object,
            unitOfWork.Object,
            publisher.Object);
        var requestHandler = (IRequestHandler<CreateBookingCommand>)handler;

        var bookingId = Guid.NewGuid();
        var command = new CreateBookingCommand(
            bookingId,
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-1)),
            DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1)),
            [
                new BookingLineItem(bookingId, 101, 2, 0, 2, new Money(100m, "EUR"))
            ],
            null);

        Assert.ThrowsAsync<BusinessRuleException>(async () =>
            await requestHandler.Handle(command, CancellationToken.None));

        inventoryGateway.Verify(
            x => x.CheckAvailabilityAsync(It.IsAny<long>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public void Handle_WhenItemsAreEmpty_ShouldFailBeforeAvailabilityCheck()
    {
        var inventoryGateway = new Mock<IInventoryGateway>();
        var repository = new Mock<IBookingRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.SetupGet(x => x.Bookings).Returns(repository.Object);
        var publisher = new Mock<IPublisher>();

        var handler = new CreateBookingCommandHandler(
            inventoryGateway.Object,
            unitOfWork.Object,
            publisher.Object);
        var requestHandler = (IRequestHandler<CreateBookingCommand>)handler;

        var command = new CreateBookingCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1)),
            DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(3)),
            [],
            null);

        Assert.ThrowsAsync<BusinessRuleException>(async () =>
            await requestHandler.Handle(command, CancellationToken.None));

        inventoryGateway.Verify(
            x => x.CheckAvailabilityAsync(It.IsAny<long>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public void Handle_WhenNightlyPriceIsNotPositive_ShouldFailBeforeAvailabilityCheck()
    {
        var inventoryGateway = new Mock<IInventoryGateway>();
        var repository = new Mock<IBookingRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.SetupGet(x => x.Bookings).Returns(repository.Object);
        var publisher = new Mock<IPublisher>();

        var handler = new CreateBookingCommandHandler(
            inventoryGateway.Object,
            unitOfWork.Object,
            publisher.Object);
        var requestHandler = (IRequestHandler<CreateBookingCommand>)handler;

        var bookingId = Guid.NewGuid();
        var command = new CreateBookingCommand(
            bookingId,
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1)),
            DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(3)),
            [
                new BookingLineItem(bookingId, 101, 2, 0, 2, new Money(0m, "EUR"))
            ],
            null);

        Assert.ThrowsAsync<BusinessRuleException>(async () =>
            await requestHandler.Handle(command, CancellationToken.None));

        inventoryGateway.Verify(
            x => x.CheckAvailabilityAsync(It.IsAny<long>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
