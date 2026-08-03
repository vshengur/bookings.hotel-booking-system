using AutoMapper;

using BookingService.Application;
using BookingService.Application.DTOs;
using BookingService.Domain.Aggregates.Booking;

using System;
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
            .ForMember(d => d.TotalPrice, opt => opt.MapFrom(s => $"{s.TotalPrice.Amount} {s.TotalPrice.Currency}"))
            .ForMember(d => d.CreatedAt, opt => opt.MapFrom(s => s.CreatedAtUtc))
            .ForMember(d => d.PaymentExpiresAt, opt => opt.MapFrom(s =>
                s.Status == BookingStatus.AwaitingPayment || s.Status == BookingStatus.Created
                    ? (DateTime?)(s.CreatedAtUtc + BookingConstants.PaymentTimeout)
                    : null));
    }
}
