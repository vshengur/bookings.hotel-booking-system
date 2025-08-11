using AutoMapper;
using BookingService.Domain.Entities;
using BookingService.Application.DTOs;

namespace BookingService.Application.Profiles;

public class BookingProfile : Profile
{
    public BookingProfile()
    {
        CreateMap<Booking, BookingDto>()
            .ForMember(d => d.CheckIn, opt => opt.MapFrom(s => s.CheckInDate))
            .ForMember(d => d.CheckOut, opt => opt.MapFrom(s => s.CheckOutDate))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.TotalPrice, opt => opt.MapFrom(s => $"{s.TotalPrice.Amount} {s.TotalPrice.Currency}"));
    }
}
