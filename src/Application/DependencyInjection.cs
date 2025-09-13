using BookingService.Application.Handlers;
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

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<ApplicationAssemblyMarker>());

        return services;
    }
}