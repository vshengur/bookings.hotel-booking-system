FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /repo

# Local NuGet packages (common/packages) must be available before restore
COPY common/packages/ ./common/packages/
COPY nuget.config ./nuget.config
COPY proto/ ./proto/

# Copy solution source
COPY services/bookings-service/src/ ./services/bookings-service/src/

WORKDIR /repo/services/bookings-service/src
RUN dotnet publish Api/BookingService.Api.csproj -c Release -o /publish --no-self-contained

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "BookingService.Api.dll"]
