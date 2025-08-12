using BookingService.Application.CommandHandlers;
using BookingService.Application.Profiles;

using Microsoft.Extensions.DependencyInjection;

using System;

namespace BookingService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // AutoMapper – собираем все профили из сборки Application
        services.AddAutoMapper(_ => { }, typeof(BookingProfile).Assembly);

        // Command-handler’ы
        services.AddScoped<CreateBookingCommandHandler>();
        services.AddScoped<ConfirmBookingCommandHandler>();
        services.AddScoped<CancelBookingCommandHandler>();

        return services;
    }
}