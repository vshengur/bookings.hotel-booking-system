using BookingService.Application.CommandHandlers;
using BookingService.Application.Pricing;
using BookingService.Application.Profiles;
using BookingService.Domain.Patterns;

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

        // Стратегии
        services.AddScoped<StandardPricingStrategy>();   // базовая
        services.AddScoped<SeasonalPricingStrategy>();   // «высокий сезон»
        //services.AddScoped<OccupancyPricingStrategy>();  // от загрузки %
        //services.AddScoped<DiscountPricingStrategy>();   // промо-код, лояльность

        services.AddTransient<Func<int, OccupancyPricingStrategy>>(sp => percent =>
            ActivatorUtilities.CreateInstance<OccupancyPricingStrategy>(sp, percent));
        return services;
    }
}