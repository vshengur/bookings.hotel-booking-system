using ApiGateway.Config;
using ApiGateway.Services;

using System.Net.Http.Headers;

namespace ApiGateway.Middleware;

public class AuthenticationMiddleware(RequestDelegate next, IConfiguration configuration, IServiceDiscovery serviceDiscovery)
{
    private readonly Dictionary<string, bool> _routes = RouteConfigLoader.LoadRoutes(configuration);

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower();

        // Найти префикс, соответствующий пути
        var requiresAuthorization = _routes
            .Where(route => path != null && path.StartsWith(route.Key))
            .Select(route => route.Value)
            .FirstOrDefault();

        if (!requiresAuthorization)
        {
            // Путь публичный, пропускаем без проверки
            await next(context);
            return;
        }

        // Проверяем наличие токена
        var token = context.Request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last();
        if (string.IsNullOrEmpty(token))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized");
            return;
        }

        // Валидация токена
        var tokenValidationResponse = await ValidateTokenAsync(token);
        if (tokenValidationResponse is null)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Forbidden");
            return;
        }

        if (!tokenValidationResponse.IsSuccessStatusCode)
        {
            context.Response.StatusCode = (int)tokenValidationResponse.StatusCode;
            await context.Response.WriteAsync(tokenValidationResponse.ReasonPhrase);
            return;
        }

        // Передаём запрос дальше
        await next(context);
    }

    private async Task<HttpResponseMessage> ValidateTokenAsync(string token)
    {
        // Логика валидации токена через Auth-сервис
        var authServiceAddress = await serviceDiscovery.GetServiceAddress("AUTH-SERVICE").ConfigureAwait(false);
        using var httpClient = new HttpClient {  BaseAddress = new Uri($"http://{authServiceAddress}") };
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await httpClient.GetAsync($"/validate-token");
        return response;
    }
}