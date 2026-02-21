# api-gateway

Docker build:
docker build -t vshengur/booking-api-gateway-service -f ./ApiGateway/Dockerfile .

Kubernetes control:
kubectl port-forward -n bookings svc/seq 5341:80
kubectl create configmap gateway-appsettings --from-file=ApiGateway/appsettings.json -n bookings

kubectl apply -f kubernetes/seq-configmap.yaml -f kubernetes/seq-pvc.yaml -f kubernetes/seq-deployment.yaml -f kubernetes/seq-service.yaml -f kubernetes/seq-ingress.yaml
kubectl delete -f kubernetes/seq-configmap.yaml -f kubernetes/seq-pvc.yaml -f kubernetes/seq-deployment.yaml -f kubernetes/seq-service.yaml -f kubernetes/seq-ingress.yaml

kubectl apply -f kubernetes/gateway-configmap.yaml -f kubernetes/gateway-deployment.yaml -f kubernetes/gateway-service.yaml -f kubernetes/gateway-ingress.yaml
kubectl delete -f kubernetes/gateway-configmap.yaml -f kubernetes/gateway-deployment.yaml -f kubernetes/gateway-service.yaml -f kubernetes/gateway-ingress.yaml