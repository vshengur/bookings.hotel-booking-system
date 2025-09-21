using Bookings.Common;

using BookingService.Infrastructure.Messaging.MassTransit;
using BookingService.Infrastructure.Persistence;

using MassTransit;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using MongoDB.Driver.Core.Configuration;

using Polly;

using System;

namespace BookingService.Infrastructure.Messaging
{
    public static class EventBusConfigurator
    {
        public static IServiceCollection AddEventBus(this IServiceCollection services, IConfiguration cfg)
        {
            services.AddMassTransit(x =>
            {
                var mongoConnection = cfg.GetConnectionString("Mongo") ?? "mongodb://localhost";
                var mongoBookingCollection = cfg.GetValue<string>("Mongo:Database") ?? "booking_saga";
                var rabbitMqHost = cfg.GetConnectionString("RabbitMQ") ?? "amqp://rabbitmq";

                x.SetKebabCaseEndpointNameFormatter();

                // Entity-Framework Outbox на уровне конфигурации, а не внутри UsingRabbitMq
                // Теперь метод распознаётся
                x.AddEntityFrameworkOutbox<BookingDbContext>(o =>
                {
                    o.UsePostgres();
                    o.QueryDelay = TimeSpan.FromSeconds(10);
                    o.UseBusOutbox();
                    // o.DisableInboxCleanupService(); // если потребуется отдельная служба чистки
                });

                // Outbox + Retry + DLQ (см. ниже)
                x.UsingRabbitMq((ctx, busCfg) =>
                {
                    busCfg.Host(new Uri(rabbitMqHost), "/", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });

                    // Глобальный Retry (5 попыток, экспонента)
                    busCfg.UseMessageRetry(r => r.Exponential(
                        retryLimit: 5,
                        minInterval: TimeSpan.FromSeconds(1),
                        maxInterval: TimeSpan.FromSeconds(30),
                        intervalDelta: TimeSpan.FromSeconds(5)));

                    // DLX (alternate-exchange) для всех сообщений
                    busCfg.Publish<IDomainEvent>(x =>
                    {
                        x.SetExchangeArgument("alternate-exchange", "booking-dlx");
                    });

                    busCfg.ConfigureEndpoints(ctx);
                });

                x.AddSagaStateMachine<BookingStateMachine, BookingState>()
                    //.InMemoryRepository(); // для демо. В проде заменить на MongoDB
                    .MongoDbRepository(r =>
                    {
                        r.Connection = mongoConnection;
                        r.DatabaseName = mongoBookingCollection;
                        r.CollectionName = "booking_state";
                    });
            });

            return services;
        }
    }
}
