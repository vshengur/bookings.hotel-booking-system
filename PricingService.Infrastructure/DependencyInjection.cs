using Consul;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PricingService.Domain.Interfaces;
using PricingService.Infrastructure.Consul;
using PricingService.Infrastructure.Data;
using PricingService.Infrastructure.Repositories;

namespace PricingService.Infrastructure;

/// <summary>
/// Dependency injection configuration for Infrastructure layer
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Database context
        services.AddDbContext<PricingDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Repositories
        services.AddScoped<IRoomPriceRepository, RoomPriceRepository>();
        services.AddScoped<IPricingRuleRepository, PricingRuleRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Consul client
        services.AddSingleton<IConsulClient>(provider => new ConsulClient(config =>
        {
            var consulAddress = configuration["Consul:Address"] ?? "http://localhost:8500";
            config.Address = new Uri(consulAddress);
        }));

        // Consul service registration
        services.AddHostedService<ConsulServiceRegistration>();

        return services;
    }
}
