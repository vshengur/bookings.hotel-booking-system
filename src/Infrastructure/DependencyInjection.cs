using Bookings.Common.Events;

using BookingService.Application.Abstractions;
using BookingService.Application.Interfaces;
using BookingService.Infrastructure.Adapters.Simulated;
using BookingService.Infrastructure.Messaging;
using BookingService.Infrastructure.Messaging.MassTransit;
using BookingService.Infrastructure.Persistence;
using BookingService.Infrastructure.UoW;

using Hangfire;
using Hangfire.PostgreSql;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using MongoDB.Driver;

using System;

namespace BookingService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var mongoConnection = configuration.GetConnectionString("Mongo") ?? "mongodb://localhost";
        services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnection));

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<InfrastructureAssemblyMarker>());

        // Gateways (simulated). В проде заменить на реальные HTTP/gRPC/Bus-адаптеры
        services.AddScoped<IPaymentGateway, PaymentGatewaySimulated>();
        services.AddScoped<IPmsGateway, PmsGatewaySimulated>();
        services.AddScoped<IInventoryGateway, InventoryGatewaySimulated>();

        // ───── переменные окружения / .env ─────
        var postgresConnection = configuration.GetConnectionString("Postgres");

        // DbContext + репозиторий
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        services.AddDbContextPool<BookingDbContext>(o => o
            .UseNpgsql(postgresConnection)
            .UseSnakeCaseNamingConvention(), poolSize: 256);

        //services.AddDbContext<BookingDbContext>(o => o
        //    .UseNpgsql(postgresConnection)
        //    .UseSnakeCaseNamingConvention());

        // MassTransit / RabbitMQ
        services.AddEventBus(configuration);
        
        // Hangfire
        services.AddHangfire(cfg =>
        {
            cfg.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(
                    cf => cf.UseNpgsqlConnection(postgresConnection),
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

        // domain events
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        return services;
    }
}