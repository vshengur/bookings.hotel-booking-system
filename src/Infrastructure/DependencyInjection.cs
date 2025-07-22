using BookingService.Application.Interfaces;
using BookingService.Application.Pricing;
using BookingService.Domain.Events;
using BookingService.Infrastructure.Messaging;
using BookingService.Infrastructure.Persistence;

using Hangfire;
using Hangfire.PostgreSql;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using System;

namespace BookingService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString,
        string rabbitMqHost)
    {
        // DbContext + репозиторий
        services.AddDbContext<BookingDbContext>(o => o
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention());
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IPricingRuleRepository, PricingRuleRepository>();

        // MassTransit / RabbitMQ
        services.AddEventBus(rabbitMqHost);
        
        // Hangfire
        services.AddHangfire(cfg =>
        {
            cfg.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(
                    cf => cf.UseNpgsqlConnection(connectionString),
                    options: new PostgreSqlStorageOptions
                    {
                        QueuePollInterval = TimeSpan.FromSeconds(3),
                        PrepareSchemaIfNecessary = true,
                        SchemaName = "Hangfire"
                    })
                .WithJobExpirationTimeout(TimeSpan.FromHours(1000));
        }).AddHangfireServer(option =>
        {
            option.SchedulePollingInterval = TimeSpan.FromSeconds(1);
        });

        // Pricing
        services.AddMemoryCache();
        services.AddScoped<IPricingRuleRepository, PricingRuleRepository>();
        services.AddScoped<IPricingStrategyProvider, PricingStrategyProvider>();

        // domain events
        services.AddScoped<IDomainEventDispatcher, MassTransitDomainEventDispatcher>();

        return services;
    }
}