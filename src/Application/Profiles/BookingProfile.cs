using AutoMapper;

using BookingService.Application.DTOs;
using BookingService.Domain.Aggregates.Booking;

using System.Linq;

namespace BookingService.Application.Profiles;

public class BookingProfile : Profile
{
    public BookingProfile()
    {
        CreateMap<Booking, BookingDto>()
            .ForMember(d => d.RoomId, opt => opt.MapFrom(s => s.Items.Select(i => i.RoomId).FirstOrDefault()))
            .ForMember(d => d.CheckIn, opt => opt.MapFrom(s => s.CheckInDate))
            .ForMember(d => d.CheckOut, opt => opt.MapFrom(s => s.CheckOutDate))
            .ForMember(d => d.TotalPrice, opt => opt.MapFrom(s => $"{s.TotalPrice.Amount} {s.TotalPrice.Currency}"));
    }
}
