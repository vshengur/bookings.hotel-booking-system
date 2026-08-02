using Prometheus;
using Serilog;
using PricingService.Application;
using PricingService.Infrastructure;
using PricingService.Middleware;
using PricingService.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/pricing-service-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Add gRPC
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

// Add application and infrastructure services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Use Serilog request logging
app.UseSerilogRequestLogging();
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Prometheus metrics middleware
app.UseMetricServer();
app.UseHttpMetrics();

app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapGrpcService<PricingGrpcService>();
app.MapHealthChecks("/health");

// Enable gRPC reflection in development
if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

Log.Information("Pricing Service starting...");

app.Run();
