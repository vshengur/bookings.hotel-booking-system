# auth-service

## Using:

### Docker Compose:

- docker-compose -f docker-compose.auth.yaml up --build

### Kubernetes:

- kubectl apply -f secrets.yaml
- kubectl apply -f postgres-deployment.yaml
- kubectl apply -f api-gateway-deployment.yaml
