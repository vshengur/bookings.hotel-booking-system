FROM mcr.microsoft.com/dotnet/sdk:9.0-bookworm-slim AS build
WORKDIR /repo

# Local NuGet packages must be available before restore
COPY common/packages/ ./common/packages/
COPY nuget.config ./nuget.config
COPY proto/ ./proto/

# Copy service source
COPY services/payment-service/src/ ./services/payment-service/src/

WORKDIR /repo/services/payment-service/src
RUN dotnet publish PaymentService.API/PaymentService.API.csproj -c Release -o /publish --no-self-contained

FROM mcr.microsoft.com/dotnet/aspnet:9.0-bookworm-slim
WORKDIR /app
COPY --from=build /publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "PaymentService.API.dll"]
