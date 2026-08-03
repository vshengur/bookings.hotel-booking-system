using Consul;
using Microsoft.Extensions.Primitives;

using Yarp.ReverseProxy.Configuration;

using DestinationConfig = Yarp.ReverseProxy.Configuration.DestinationConfig;
using RouteConfig = Yarp.ReverseProxy.Configuration.RouteConfig;

namespace ApiGateway.Config;

/// <summary>
/// Реализация динамической конфигурации YARP с использованием Consul
/// </summary>
public class ConsulProxyConfigProvider(IConsulClient consulClient) : IProxyConfigProvider
{
    public IProxyConfig GetConfig()
    {
        // Только сервисы, явно зарегистрировавшие маршрутный prefix в Consul metadata
        var proxied = consulClient.Agent.Services().Result.Response.Values
            .Where(s => s.Meta.ContainsKey("prefix"))
            .ToList();

        var routes = proxied.Select(service => new RouteConfig
        {
            RouteId = service.ID,
            ClusterId = $"{service.ID}-cluster",
            Match = new RouteMatch
            {
                Path = $"/{service.Meta["prefix"]}/{{**catch-all}}"
            },
            Transforms =
            [
                new Dictionary<string, string> { { "RequestHeadersCopy", "true" } },
                new Dictionary<string, string> { { "PathRemovePrefix", $"/{service.Meta["prefix"]}" } }
            ]
        }).ToList();

        var clusters = proxied.Select(service => new ClusterConfig
        {
            ClusterId = $"{service.ID}-cluster",
            Destinations = new Dictionary<string, DestinationConfig>
            {
                [service.ID] = new DestinationConfig
                {
                    Address = $"http://{service.Address}:{service.Port}"
                }
            }
        }).ToList();

        // Возвращаем новую конфигурацию
        return new ConsulProxyConfig(routes, clusters);
    }
}

// Класс для хранения динамической конфигурации YARP
public class ConsulProxyConfig(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters) : IProxyConfig
{
    public IReadOnlyList<RouteConfig> Routes { get; } = routes;
    public IReadOnlyList<ClusterConfig> Clusters { get; } = clusters;
    public IChangeToken ChangeToken { get; } = new CancellationChangeToken(new CancellationToken());
}