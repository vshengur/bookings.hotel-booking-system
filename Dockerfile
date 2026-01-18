# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files
COPY ["PricingService/PricingService.csproj", "PricingService/"]
COPY ["PricingService.Domain/PricingService.Domain.csproj", "PricingService.Domain/"]
COPY ["PricingService.Application/PricingService.Application.csproj", "PricingService.Application/"]
COPY ["PricingService.Infrastructure/PricingService.Infrastructure.csproj", "PricingService.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "PricingService/PricingService.csproj"

# Copy all files
COPY . .

# Build the application
WORKDIR "/src/PricingService"
RUN dotnet build "PricingService.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "PricingService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Create non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy published files
COPY --from=publish /app/publish .

# Create logs directory and set permissions
RUN mkdir -p /app/logs && chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose ports
EXPOSE 5003
EXPOSE 5103

# Environment variables
ENV ASPNETCORE_URLS=http://+:5003
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:5003/health || exit 1

ENTRYPOINT ["dotnet", "PricingService.dll"]
