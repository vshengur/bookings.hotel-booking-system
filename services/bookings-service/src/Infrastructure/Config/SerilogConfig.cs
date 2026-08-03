using Microsoft.Extensions.Hosting;
using Serilog;

namespace BookingService.Infrastructure.Config
{
    public static class SerilogConfig
    {
        public static IHostBuilder UseSerilogConfig(this IHostBuilder builder) =>
            builder.UseSerilog((ctx, cfg) => 
            cfg.ReadFrom.Configuration(ctx.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("service", "booking-service"));
    }
}
