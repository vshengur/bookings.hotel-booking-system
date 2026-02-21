# Consul - Configuration & Service Discovery

Centralized configuration and secrets management for Hotel Booking System.

## 🚀 Quick Start

```bash
# One command to set everything up
./start-consul.sh
```

See [QUICKSTART.md](QUICKSTART.md) for details.

## 📚 Documentation

- **[QUICKSTART.md](QUICKSTART.md)** - Quick start guide
- **[SECRETS_MANAGEMENT.md](SECRETS_MANAGEMENT.md)** - Complete secrets management guide

## Features

✅ **Automatic JWT Secret Generation** - No manual setup needed for dev
✅ **Multi-Environment Support** - Dev, UAT, Production configs
✅ **Centralized Configuration** - All services read from one source
✅ **Secure Secret Storage** - Consul KV Store
✅ **Auto-Initialization** - Secrets loaded on startup

## Manual Setup

```bash
# 1. Create secrets file
cp secrets.env.example secrets.env

# 2. Edit secrets (optional for dev)
nano secrets.env

# 3. Start Consul
docker-compose -f docker-compose.consul.yml up -d

# 4. Verify
curl http://localhost:8500/v1/kv/config/?keys
```

## Kubernetes Deployment

```bash
# Apply configurations
kubectl apply -f kubernetes/consul-config.yaml \
              -f kubernetes/consul-storage.yaml \
              -f kubernetes/consul-deployment.yaml \
              -f kubernetes/consul-service.yaml

# Restart to apply new configs
kubectl rollout restart deployment consul -n bookings

# Delete deployment
kubectl delete -f kubernetes/consul-config.yaml \
               -f kubernetes/consul-storage.yaml \
               -f kubernetes/consul-deployment.yaml \
               -f kubernetes/consul-service.yaml
```

## Access

- **UI**: http://localhost:8500/ui
- **API**: http://localhost:8500/v1/
- **DNS**: localhost:8600

## Troubleshooting

See [SECRETS_MANAGEMENT.md#troubleshooting](SECRETS_MANAGEMENT.md#troubleshooting) for common issues.