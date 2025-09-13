using AutoMapper;

using Bookings.Common.ValueObjects;

using BookingService.Application.Commands;
using BookingService.Application.DTOs;
using BookingService.Application.Interfaces;
using BookingService.Domain.Aggregates.Booking;

using MediatR;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookingService.Api.Controllers
{
    public record MoneyDto(decimal Amount, string Currency = "EUR");
    public record BookingLineItemDto(Guid RoomId, int Adults, int Children, int Nights, MoneyDto PricePerNight);
    public record CreateBookingRequest(Guid BookingId, Guid GuestId, DateOnly CheckIn, DateOnly CheckOut, List<BookingLineItemDto> Items, string? PromoCode);

    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingRepository _repo;
        private readonly IMapper _mapper;
        private readonly ISender _sender;

        public BookingController(
            IBookingRepository repo,
            ISender sender,
            IMapper mapper)
        {
            _repo = repo;
            _sender = sender;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IResult> Create([FromBody] CreateBookingRequest req)
        {
            var id = req.BookingId == Guid.Empty ? Guid.NewGuid() : req.BookingId;
            var items = req.Items
                .Select(i => new BookingLineItem(id, i.RoomId, i.Adults, i.Children, i.Nights,
                    new Money(i.PricePerNight.Amount, i.PricePerNight.Currency)))
                .ToList();

            await _sender.Send(new CreateBookingCommand(
                id, req.GuestId, req.CheckIn, req.CheckOut, items, req.PromoCode));

            return Results.Accepted($"/bookings/{id}", new { bookingId = id });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BookingDto>> Get(Guid id)
        {
            var booking = await _repo.GetAsync(id);
            return booking is not null
                ? Ok(_mapper.Map<BookingDto>(booking))
                : NotFound();
        }

        [HttpPost("{id}/confirm")]
        public async Task<IActionResult> Confirm(Guid id)
        {
            //await _confirmHandler.Handle(new ConfirmBookingCommand(id));
            return NoContent();
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(Guid id, string reason)
        {
            //await _cancelHandler.Handle(new CancelBookingCommand(id, reason));
            return NoContent();
        }
    }
}
