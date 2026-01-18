using Microsoft.Extensions.DependencyInjection;
using PricingService.Application.Services;
using PricingService.Application.Strategies;
using PricingService.Domain.Interfaces;

namespace PricingService.Application;

/// <summary>
/// Dependency injection configuration for Application layer
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register services
        services.AddScoped<IPricingService, Services.PricingService>();
        services.AddScoped<IPricingRuleService, PricingRuleService>();

        // Register pricing strategies
        services.AddScoped<IPricingStrategy, BasePricingStrategy>();
        services.AddScoped<IPricingStrategy, SeasonalPricingStrategy>();
        services.AddScoped<IPricingStrategy, WeekendPricingStrategy>();
        services.AddScoped<IPricingStrategy, PromotionalPricingStrategy>();
        services.AddScoped<IPricingStrategy, DynamicPricingStrategy>();

        return services;
    }
}
