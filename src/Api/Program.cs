using BookingService.Application;
using BookingService.Infrastructure;
using BookingService.Infrastructure.Config;
using BookingService.Infrastructure.Persistence;

using Hangfire;

using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilogConfig();

// ───── переменные окружения / .env ─────
var connStr = builder.Configuration.GetConnectionString("Default");
var rabbitMqHost = builder.Configuration["RabbitMq:Host"] ?? "localhost";

// ───── Слои ─────
builder.Services
        .AddApplication()
        .AddInfrastructure(connStr!, rabbitMqHost);

// SignalR
builder.Services.AddSignalR();

// Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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

app.Run();
