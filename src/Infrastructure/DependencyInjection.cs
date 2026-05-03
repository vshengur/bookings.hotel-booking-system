using Bookings.Common.Events;

using BookingService.Application.Abstractions;
using BookingService.Application.Interfaces;
using BookingService.Infrastructure.Adapters;
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

using PaymentService.Contracts.Grpc.V1;

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

        // ───── gRPC клиент для Payment Service ─────
        var paymentServiceUrl = configuration["PaymentService:Url"] ?? "http://localhost:60400";
        services.AddGrpcClient<PaymentService.Contracts.Grpc.V1.PaymentService.PaymentServiceClient>(options =>
        {
            options.Address = new Uri(paymentServiceUrl);
        });

        var roomServiceUrl = configuration["RoomService:Url"] ?? "http://localhost:8081";
        services.AddHttpClient<RoomServiceInventoryGateway>(client =>
        {
            client.BaseAddress = new Uri(roomServiceUrl);
        });

        // Gateways
        // Используем gRPC клиент для Payment Gateway, PMS пока simulated
        services.AddScoped<IPaymentGateway, PaymentGatewayGrpc>();
        services.AddScoped<IPmsGateway, PmsGatewaySimulated>();
        services.AddScoped<IInventoryGateway>(sp => sp.GetRequiredService<RoomServiceInventoryGateway>());

        // ───── переменные окружения / .env ─────
        var postgresConnection = configuration.GetConnectionString("Postgres");

        // DbContext + репозиторий
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        services.AddDbContextFactory<BookingDbContext>(o => o
            .UseNpgsql(postgresConnection)
            .UseSnakeCaseNamingConvention(), lifetime: ServiceLifetime.Scoped);

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
