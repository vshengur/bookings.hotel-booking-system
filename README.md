# auth-service

## Using:

### Docker Compose:

- docker-compose up --build

### Kubernetes:

- kubectl apply -f secrets.yaml
- kubectl apply -f postgres-deployment.yaml
- kubectl apply -f api-gateway-deployment.yaml
