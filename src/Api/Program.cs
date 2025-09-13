using BookingService.Application;
using BookingService.Infrastructure;
using BookingService.Infrastructure.Config;
using BookingService.Infrastructure.Persistence;

using Hangfire;

using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilogConfig();

// ───── Слои ─────
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

// SignalR
builder.Services.AddSignalR();

// Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ───── применяем миграции ─────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
    db.Database.Migrate();
}

app.UseAuthorization();

app.MapControllers();
app.MapHangfireDashboard("/hangfire");
app.MapHub<BookingService.Api.Hubs.BookingHub>("/hubs/booking");

app.UseHealthChecks("/health");

app.Run();
