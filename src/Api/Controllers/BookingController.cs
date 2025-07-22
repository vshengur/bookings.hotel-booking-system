using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BookingService.Application.Commands;
using BookingService.Application.CommandHandlers;
using BookingService.Application.DTOs;
using AutoMapper;
using BookingService.Application.Interfaces;

namespace BookingService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly CreateBookingCommandHandler _createHandler;
        private readonly CancelBookingCommandHandler _cancelHandler;
        private readonly ConfirmBookingCommandHandler _confirmHandler;
        private readonly IBookingRepository _repo;
        private readonly IMapper _mapper;

        public BookingController(CreateBookingCommandHandler createHandler, CancelBookingCommandHandler cancelHandler,
                                 ConfirmBookingCommandHandler confirmHandler, IBookingRepository repo, IMapper mapper)
        {
            _createHandler = createHandler;
            _cancelHandler = cancelHandler;
            _confirmHandler = confirmHandler;
            _repo = repo;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBookingCommand cmd)
        {
            var id = await _createHandler.Handle(cmd);
            return CreatedAtAction(nameof(Get), new { id }, id);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BookingDto>> Get(Guid id)
        {
            var booking = await _repo.GetAsync(id);
            return booking is null ? NotFound() : Ok(_mapper.Map<BookingDto>(booking));
        }

        [HttpPost("{id}/confirm")]
        public async Task<IActionResult> Confirm(Guid id)
        {
            await _confirmHandler.Handle(new ConfirmBookingCommand(id));
            return NoContent();
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(Guid id, string reason)
        {
            await _cancelHandler.Handle(new CancelBookingCommand(id, reason));
            return NoContent();
        }
    }
}
