# Pricing Service

A microservice for managing room pricing and calculating dynamic prices based on various pricing strategies for the Hotel Booking System.

## Features

- **Clean Architecture**: Organized into Domain, Application, Infrastructure, and API layers
- **Strategy Pattern**: Implements multiple pricing strategies (Base, Seasonal, Weekend, Promotional, Dynamic)
- **REST API**: Full CRUD operations for room prices and pricing rules
- **gRPC Service**: Inter-service communication for price calculations
- **PostgreSQL Database**: Entity Framework Core with code-first migrations
- **Consul Service Discovery**: Automatic service registration and health checks
- **Prometheus Metrics**: Built-in metrics endpoint for monitoring
- **Serilog Logging**: Structured logging to console and file
- **Docker Support**: Containerized deployment with docker-compose

## Technologies

- .NET 10.0
- ASP.NET Core Web API
- gRPC
- Entity Framework Core 10.0
- PostgreSQL 16
- Consul 1.19
- Prometheus
- Serilog
- Docker & Docker Compose

## Architecture

### Project Structure

```
pricing-service/
├── PricingService/                 # API Layer
│   ├── Controllers/               # REST API Controllers
│   ├── Services/                  # gRPC Service Implementation
│   ├── Protos/                    # gRPC Proto files
│   └── appsettings.json          # Configuration
├── PricingService.Domain/         # Domain Layer
│   ├── Entities/                 # Domain Entities
│   └── Interfaces/               # Domain Interfaces
├── PricingService.Application/    # Application Layer
│   ├── DTOs/                     # Data Transfer Objects
│   ├── Services/                 # Application Services
│   └── Strategies/               # Pricing Strategies
└── PricingService.Infrastructure/ # Infrastructure Layer
    ├── Data/                     # DbContext & Migrations
    ├── Repositories/             # Repository Implementations
    └── Consul/                   # Consul Integration
```

### Pricing Strategies

1. **Base Strategy**: Returns base price without modifications
2. **Seasonal Strategy**: Adjusts prices based on season
   - Peak (Jun-Aug): 1.5x
   - High (Apr-May, Sep-Oct): 1.3x
   - Low (Nov-Mar): 0.9x
3. **Weekend Strategy**: Premium pricing for Friday/Saturday nights (1.2x)
4. **Promotional Strategy**: Applies discounts based on promo codes or long stays
   - 7+ nights: 10% off
   - 14+ nights: 15% off
   - 30+ nights: 20% off
5. **Dynamic Strategy**: Adjusts prices based on occupancy
   - >80% occupancy: 1.4x
   - 60-80% occupancy: 1.2x
   - 40-60% occupancy: 1.0x
   - <40% occupancy: 0.85x

## API Documentation

### REST API Endpoints

#### Pricing Operations

**Calculate Price**
```http
POST /api/pricing/calculate
Content-Type: application/json

{
  "roomId": 1,
  "roomType": "Deluxe",
  "checkIn": "2026-07-15T14:00:00Z",
  "checkOut": "2026-07-20T11:00:00Z",
  "occupancy": 75,
  "promoCode": "SUMMER2026"
}
```

**Get Room Price**
```http
GET /api/pricing/room/{roomId}
```

**Get All Active Prices**
```http
GET /api/pricing/active
```

**Create Room Price**
```http
POST /api/pricing/room-price
Content-Type: application/json

{
  "roomId": 1,
  "roomType": "Standard",
  "basePrice": 100.00,
  "currency": "EUR",
  "validFrom": "2026-01-01T00:00:00Z",
  "validTo": null
}
```

**Update Room Price**
```http
PUT /api/pricing/room-price/{id}
Content-Type: application/json

{
  "basePrice": 120.00,
  "validFrom": "2026-01-01T00:00:00Z",
  "validTo": null,
  "isActive": true
}
```

**Delete Room Price**
```http
DELETE /api/pricing/room-price/{id}
```

#### Pricing Rules Operations

**Get All Active Rules**
```http
GET /api/pricingrules/active
```

**Get Rule by ID**
```http
GET /api/pricingrules/{id}
```

**Get Rules by Type**
```http
GET /api/pricingrules/type/{ruleType}
```

**Create Pricing Rule**
```http
POST /api/pricingrules
Content-Type: application/json

{
  "ruleName": "Summer Special",
  "ruleType": "Seasonal",
  "multiplier": 1.5,
  "discountPercent": null,
  "validFrom": "2026-06-01T00:00:00Z",
  "validTo": "2026-08-31T23:59:59Z",
  "minNights": null,
  "priority": 5,
  "conditions": null
}
```

### gRPC API

**Service Definition**: `Protos/pricing.proto`

**Methods**:
- `CalculatePrice`: Calculate price for a booking
- `GetRoomPrice`: Get room price by room ID
- `GetAllActivePrices`: Get all active room prices

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=pricing_db;Username=postgres;Password=postgres"
  },
  "Consul": {
    "Address": "http://localhost:8500",
    "ServiceName": "pricing-service",
    "ServiceId": "pricing-service-1",
    "ServiceAddress": "localhost",
    "ServicePort": "5003"
  },
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:5003"
      },
      "Grpc": {
        "Url": "http://0.0.0.0:5103",
        "Protocols": "Http2"
      }
    }
  }
}
```

## Getting Started

### Prerequisites

- .NET 10.0 SDK
- PostgreSQL 16+
- Docker & Docker Compose (optional)
- Consul (optional, for service discovery)

### Local Development

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd pricing-service
   ```

2. **Update database connection string** in `appsettings.Development.json`

3. **Run database migrations**
   ```bash
   cd PricingService
   dotnet ef database update --project ../PricingService.Infrastructure
   ```

4. **Run the service**
   ```bash
   dotnet run
   ```

5. **Access Swagger UI**
   - Navigate to `http://localhost:5003/swagger`

### Docker Deployment

1. **Build and run with docker-compose**
   ```bash
   docker-compose up -d
   ```

2. **View logs**
   ```bash
   docker-compose logs -f pricing-service
   ```

3. **Stop services**
   ```bash
   docker-compose down
   ```

### Database Migrations

**Create a new migration**
```bash
dotnet ef migrations add MigrationName --project PricingService.Infrastructure --startup-project PricingService
```

**Update database**
```bash
dotnet ef database update --project PricingService.Infrastructure --startup-project PricingService
```

**Remove last migration**
```bash
dotnet ef migrations remove --project PricingService.Infrastructure --startup-project PricingService
```

## Monitoring

### Health Check
```http
GET /health
```

### Prometheus Metrics
```http
GET /metrics
```

### Prometheus Dashboard
- URL: `http://localhost:9090`

### Consul UI
- URL: `http://localhost:8500`

## Seed Data

The service comes with pre-seeded data:

### Room Prices
- Standard: €100/night
- Deluxe: €150/night
- Suite: €250/night
- Presidential: €500/night

### Pricing Rules
- Summer Peak Season (Jun-Aug): 1.5x multiplier
- Weekend Premium: 1.2x multiplier
- Long Stay Discount (7+ nights): 15% off
- Early Bird Special: 20% off

## Testing

### Using curl

**Calculate Price**
```bash
curl -X POST http://localhost:5003/api/pricing/calculate \
  -H "Content-Type: application/json" \
  -d '{
    "roomId": 2,
    "roomType": "Deluxe",
    "checkIn": "2026-07-15T14:00:00Z",
    "checkOut": "2026-07-20T11:00:00Z",
    "occupancy": 75
  }'
```

**Get Room Price**
```bash
curl http://localhost:5003/api/pricing/room/1
```

### Using grpcurl

**Calculate Price (gRPC)**
```bash
grpcurl -plaintext -d '{
  "room_id": 1,
  "room_type": "Standard",
  "check_in": "2026-07-15T14:00:00Z",
  "check_out": "2026-07-20T11:00:00Z",
  "occupancy": 75
}' localhost:5103 pricing.PricingGrpc/CalculatePrice
```

## Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Environment name | `Development` |
| `ASPNETCORE_URLS` | Service URLs | `http://+:5003` |
| `ConnectionStrings__DefaultConnection` | Database connection | See appsettings.json |
| `Consul__Address` | Consul server URL | `http://localhost:8500` |
| `Consul__ServiceName` | Service name in Consul | `pricing-service` |
| `Consul__ServicePort` | Service port | `5003` |

## Ports

- **5003**: HTTP/REST API
- **5103**: gRPC API
- **5432**: PostgreSQL
- **8500**: Consul HTTP API
- **9090**: Prometheus

## License

This project is part of the Hotel Booking System.

## Contributing

Please read the contribution guidelines in the main repository.

## Support

For issues and questions, please open an issue in the main repository.
