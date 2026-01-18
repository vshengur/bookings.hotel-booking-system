# Room Service

Room Service manages hotel rooms, their availability, and reservations for the Hotel Booking System.

## Features

- **Room Management**: CRUD operations for rooms
- **Room Search**: Advanced search with filters (dates, capacity, price, amenities)
- **Availability Management**: Check availability, reserve, and release rooms
- **gRPC API**: For inter-service communication
- **REST API**: For external clients
- **Consul Integration**: Service discovery and health checks
- **Prometheus Metrics**: Performance monitoring
- **Structured Logging**: Zap logger with JSON output

## Technology Stack

- **Language**: Go 1.23.4
- **Framework**: Gin (HTTP), gRPC
- **Database**: PostgreSQL 16
- **ORM**: GORM
- **Service Discovery**: Consul
- **Logging**: Zap
- **Metrics**: Prometheus
- **Containerization**: Docker

## Architecture Patterns

This service implements several design patterns:

- **Builder Pattern**: Room construction (`RoomBuilder`)
- **Strategy Pattern**: Pricing strategies (Base, Seasonal, Dynamic, Promotional)
- **Repository Pattern**: Data access abstraction
- **Decorator Pattern**: Middleware for logging and metrics
- **Factory Pattern**: Service creation
- **Clean Architecture**: Separation of concerns (Domain, Application, Infrastructure, API)

## Project Structure

```
room-service/
├── cmd/
│   └── server/          # Application entry point
├── internal/
│   ├── config/          # Configuration
│   ├── handlers/        # HTTP handlers (API layer)
│   ├── middleware/      # HTTP middleware
│   ├── models/          # Domain models and DTOs
│   ├── repository/      # Data access layer
│   ├── services/        # Business logic layer
│   └── utils/           # Utilities (Consul client)
├── pkg/
│   └── logger/          # Logger package
├── proto/               # gRPC proto definitions
├── migrations/          # Database migrations
├── tests/               # Tests
│   ├── unit/
│   ├── integration/
│   └── load/
├── Dockerfile
├── docker-compose.yml
└── README.md
```

## API Endpoints

### REST API

#### Room CRUD
- `POST /api/rooms` - Create room (admin)
- `GET /api/rooms` - Get all rooms (with pagination)
- `GET /api/rooms/:id` - Get room by ID
- `PUT /api/rooms/:id` - Update room (admin)
- `DELETE /api/rooms/:id` - Delete room (admin)

#### Search
- `GET /api/rooms/search` - Search rooms with filters

Query parameters:
- `checkIn` (datetime) - Check-in date
- `checkOut` (datetime) - Check-out date
- `adults` (int) - Number of adults
- `children` (int) - Number of children
- `minPrice` (float) - Minimum price
- `maxPrice` (float) - Maximum price
- `roomType` (string) - Room type filter
- `minCapacity` (int) - Minimum capacity
- `amenities` ([]string) - Required amenities
- `page` (int) - Page number (default: 1)
- `pageSize` (int) - Page size (default: 10, max: 100)
- `sortBy` (string) - Sort order (price_asc, price_desc, capacity_asc, capacity_desc)

#### Availability
- `GET /api/rooms/:id/availability` - Check room availability
- `POST /api/rooms/:id/reserve` - Reserve room (creates 15-min hold)
- `POST /api/rooms/:id/release` - Release room reservation

### Health & Metrics
- `GET /health` - Health check
- `GET /metrics` - Prometheus metrics

### gRPC API

See `proto/room.proto` for complete gRPC service definition.

## Database Schema

### Tables
- `rooms` - Room information
- `room_amenities` - Room amenities
- `room_images` - Room images
- `room_availabilities` - Room reservations and availability

See `migrations/` directory for detailed schema.

## Configuration

Environment variables:

```bash
# Server
SERVER_HOST=0.0.0.0
SERVER_PORT=8083
GRPC_PORT=50053

# Database
DB_HOST=localhost
DB_PORT=5432
DB_USER=postgres
DB_PASSWORD=postgres
DB_NAME=room_service_db
DB_SSL_MODE=disable

# Consul
CONSUL_HOST=localhost
CONSUL_PORT=8500
CONSUL_SERVICE_NAME=room-service
CONSUL_SERVICE_ID=room-service-1

# Logger
LOG_LEVEL=info
LOG_OUTPUT_PATH=stdout
```

## Running Locally

### Prerequisites
- Go 1.23.4+
- PostgreSQL 16
- Consul (for service discovery)
- Docker (optional)

### Using Docker Compose

```bash
# Start the service and database
docker-compose up -d

# View logs
docker-compose logs -f room-service

# Stop the service
docker-compose down
```

The service will be available at:
- HTTP API: http://localhost:8083
- gRPC: localhost:50053
- Database: localhost:5433

### Using Go

```bash
# Install dependencies
go mod download

# Run migrations manually
psql -h localhost -p 5433 -U postgres -d room_service_db -f migrations/001_create_rooms_table.sql
psql -h localhost -p 5433 -U postgres -d room_service_db -f migrations/002_create_room_amenities_table.sql
psql -h localhost -p 5433 -U postgres -d room_service_db -f migrations/003_create_room_images_table.sql
psql -h localhost -p 5433 -U postgres -d room_service_db -f migrations/004_create_room_availabilities_table.sql
psql -h localhost -p 5433 -U postgres -d room_service_db -f migrations/005_seed_data.sql

# Run the service
go run cmd/server/main.go
```

## Testing

```bash
# Run unit tests
go test ./... -v

# Run integration tests
go test ./tests/integration/... -v

# Run with coverage
go test ./... -coverprofile=coverage.out
go tool cover -html=coverage.out
```

## Example Requests

### Create Room
```bash
curl -X POST http://localhost:8083/api/rooms \
  -H "Content-Type: application/json" \
  -d '{
    "room_number": "401",
    "room_type": "deluxe",
    "floor": 4,
    "capacity": 2,
    "bed_type": "King Bed",
    "size": 35.0,
    "description": "Deluxe room with ocean view",
    "base_price": 199.99,
    "currency": "EUR",
    "amenities": [
      {
        "amenity_type": "basic",
        "name": "WiFi",
        "description": "High-speed internet"
      }
    ],
    "images": [
      {
        "url": "https://example.com/image.jpg",
        "title": "Main View",
        "is_primary": true,
        "sort_order": 1
      }
    ]
  }'
```

### Search Rooms
```bash
curl "http://localhost:8083/api/rooms/search?checkIn=2026-02-01T14:00:00Z&checkOut=2026-02-05T10:00:00Z&adults=2&minPrice=100&maxPrice=300&page=1&pageSize=10&sortBy=price_asc"
```

### Check Availability
```bash
curl "http://localhost:8083/api/rooms/1/availability?check_in=2026-02-01T14:00:00Z&check_out=2026-02-05T10:00:00Z"
```

### Reserve Room
```bash
curl -X POST http://localhost:8083/api/rooms/1/reserve \
  -H "Content-Type: application/json" \
  -d '{
    "booking_id": 12345,
    "check_in": "2026-02-01T14:00:00Z",
    "check_out": "2026-02-05T10:00:00Z"
  }'
```

## Integration with Other Services

### Booking Service
Room Service is called by Booking Service to:
1. Search available rooms
2. Reserve rooms (15-minute hold)
3. Release reservations (on booking cancellation/expiration)

### Payment Service
Room Service provides pricing information used by Payment Service for:
1. Quote calculation
2. Price breakdown

### API Gateway
All external requests are routed through API Gateway which:
1. Performs authentication/authorization
2. Rate limiting
3. Request routing to Room Service

## Monitoring

### Prometheus Metrics
- `room_service_http_requests_total` - Total HTTP requests
- `room_service_http_request_duration_seconds` - Request duration

Access metrics: http://localhost:8083/metrics

### Logging
Structured JSON logs with correlation IDs for request tracing.

## Deployment

### Docker
```bash
docker build -t room-service:latest .
docker run -p 8083:8083 -p 50053:50053 --env-file local.env room-service:latest
```

### Kubernetes
```bash
kubectl apply -f k8s/
```

## License

Part of Hotel Booking System - Educational Project

## Author

Hotel Booking System Team
