using BookingService.Application;
using BookingService.Infrastructure;
using BookingService.Infrastructure.Config;
using BookingService.Infrastructure.Persistence;

using Hangfire;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;

using System.Collections.Generic;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails(opt =>
{
    opt.CustomizeProblemDetails = ctx =>
    {
        ctx.ProblemDetails.Instance =
            $"{ctx.HttpContext.Request.Method} {ctx.HttpContext.Request.Path}";

        var activity = ctx.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;

        ctx.ProblemDetails.Extensions.TryAdd("requestId", ctx.HttpContext.TraceIdentifier);
        ctx.ProblemDetails.Extensions.TryAdd("tracrId", activity?.Id);
        ctx.ProblemDetails.Status = (int)HttpStatusCode.BadRequest;
    };
});

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
    var ctx = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
    ctx.Database.Migrate();
}

app.UseAuthorization();

app.MapControllers();
app.MapHangfireDashboard("/hangfire");
app.MapHub<BookingService.Api.Hubs.BookingHub>("/hubs/booking");

app.UseHealthChecks("/health");

app.Run();
