using Consul;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PricingService.Infrastructure.Consul;

/// <summary>
/// Consul service registration hosted service
/// </summary>
public class ConsulServiceRegistration : IHostedService
{
    private readonly IConsulClient _consulClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConsulServiceRegistration> _logger;
    private string? _registrationId;

    public ConsulServiceRegistration(
        IConsulClient consulClient,
        IConfiguration configuration,
        ILogger<ConsulServiceRegistration> logger)
    {
        _consulClient = consulClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var serviceName = _configuration["Consul:ServiceName"] ?? "pricing-service";
        var serviceId = _configuration["Consul:ServiceId"] ?? $"{serviceName}-{Guid.NewGuid()}";
        var serviceAddress = _configuration["Consul:ServiceAddress"] ?? "localhost";
        var servicePort = int.Parse(_configuration["Consul:ServicePort"] ?? "5003");
        var healthCheckUrl = $"http://{serviceAddress}:{servicePort}/health";

        _registrationId = serviceId;

        var registration = new AgentServiceRegistration
        {
            ID = serviceId,
            Name = serviceName,
            Address = serviceAddress,
            Port = servicePort,
            Tags = new[] { "pricing", "api", "grpc" },
            Check = new AgentServiceCheck
            {
                HTTP = healthCheckUrl,
                Interval = TimeSpan.FromSeconds(10),
                Timeout = TimeSpan.FromSeconds(5),
                DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1)
            }
        };

        try
        {
            await _consulClient.Agent.ServiceDeregister(registration.ID, cancellationToken);
            await _consulClient.Agent.ServiceRegister(registration, cancellationToken);
            _logger.LogInformation("Service registered with Consul: {ServiceId}", serviceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register service with Consul");
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(_registrationId))
        {
            try
            {
                await _consulClient.Agent.ServiceDeregister(_registrationId, cancellationToken);
                _logger.LogInformation("Service deregistered from Consul: {ServiceId}", _registrationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deregister service from Consul");
            }
        }
    }
}
