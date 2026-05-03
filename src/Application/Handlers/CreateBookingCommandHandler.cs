using Bookings.Common.Exceptions;

using BookingService.Application.Commands;
using BookingService.Application.Abstractions;
using BookingService.Application.Events;
using BookingService.Application.Interfaces;
using BookingService.Domain.Patterns;

using MediatR;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BookingService.Application.Handlers;

public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand>
{
    private readonly IInventoryGateway _inventoryGateway;
    private readonly IUnitOfWork _uow;
    private readonly IPublisher _publisher;

    public CreateBookingCommandHandler(
        IInventoryGateway inventoryGateway,
        IUnitOfWork uow,
        IPublisher publisher)
    {
        _inventoryGateway = inventoryGateway;
        _uow = uow;
        _publisher = publisher;
    }

    async Task IRequestHandler<CreateBookingCommand>.Handle(CreateBookingCommand request, CancellationToken ct)
    {
        ValidateRequest(request);

        foreach (var roomId in request.Items.Select(item => item.RoomId).Distinct())
            await _inventoryGateway.CheckAvailabilityAsync(roomId, request.CheckIn, request.CheckOut, ct);

        var booking = new BookingBuilder()
            .WithId(request.BookingId)
            .ForGuest(request.GuestId)
            .InPeriod(request.CheckIn, request.CheckOut)
            .Build();

        foreach (var i in request.Items)
            booking.AddLineItem(i.RoomId, i.Adults, i.Children, i.Nights, i.PricePerNight);

        await _uow.Bookings.AddAsync(booking, ct);
        await _uow.SaveChangesAsync(ct);

        await _publisher.Publish(new BookingInitiated(request.BookingId), ct);
    }

    private static void ValidateRequest(CreateBookingCommand request)
    {
        if (request.CheckIn < DateOnly.FromDateTime(DateTime.UtcNow.Date))
            throw new BusinessRuleException("Check-in date cannot be in the past");

        if (request.Items.Count == 0)
            throw new BusinessRuleException("Booking must contain at least one room");

        foreach (var item in request.Items)
        {
            if (item.PricePerNight.Amount <= 0)
                throw new BusinessRuleException($"Room {item.RoomId} must have a positive nightly price");

            if (string.IsNullOrWhiteSpace(item.PricePerNight.Currency))
                throw new BusinessRuleException($"Room {item.RoomId} must have a currency");
        }
    }
}
