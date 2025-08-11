using Bookings.Common;

using BookingService.Infrastructure.Messaging.Consumers;
using BookingService.Infrastructure.Persistence;

using MassTransit;

using Microsoft.Extensions.DependencyInjection;

using System;

namespace BookingService.Infrastructure.Messaging
{
    public static class EventBusConfigurator
    {
        public static IServiceCollection AddEventBus(this IServiceCollection services, string rabbitMqHost)
        {
            services.AddMassTransit(x =>
            {
                x.AddConsumer<PaymentConfirmedConsumer>();
                x.AddConsumer<PaymentFailedConsumer>();

                // ① Entity-Framework Outbox на уровне конфигурации, а не внутри UsingRabbitMq
                // Теперь метод распознаётся
                x.AddEntityFrameworkOutbox<BookingDbContext>(o =>
                {
                    o.UsePostgres();
                    o.QueryDelay = TimeSpan.FromSeconds(10);
                    o.UseBusOutbox();
                    // o.DisableInboxCleanupService(); // если потребуется отдельная служба чистки
                });

                // ⬇️ Outbox + Retry + DLQ (см. ниже)
                x.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host(rabbitMqHost, "/", h => { });

                    // Глобальный Retry (5 попыток, экспонента)
                    cfg.UseMessageRetry(r => r.Exponential(
                        retryLimit: 5,
                        minInterval: TimeSpan.FromSeconds(1),
                        maxInterval: TimeSpan.FromSeconds(30),
                        intervalDelta: TimeSpan.FromSeconds(5)));

                    // DLX (alternate-exchange) для всех сообщений
                    cfg.Publish<IDomainEvent>(x =>
                    {
                        x.SetExchangeArgument("alternate-exchange", "booking-dlx");
                    });

                    cfg.ConfigureEndpoints(ctx);
                });
            });

            return services;
        }
    }
}
