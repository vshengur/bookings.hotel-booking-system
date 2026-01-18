# bookings.hotel-booking-system
git clone https://github.com/vshengur/bookings.hotel-booking-system.git hotel-booking-system
git submodule add https://github.com/vshengur/bookings.auth-service.git services/auth-service
git submodule add https://github.com/vshengur/bookings.booking-service.git services/booking-service
git submodule add https://github.com/vshengur/bookings.inventory-service.git services/inventory-service
git submodule add https://github.com/vshengur/bookings.payment-service.git services/payment-service
git submodule add https://github.com/vshengur/bookings.notification-service.git services/notification-service
git submodule add https://github.com/vshengur/bookings.api-gateway.git services/api-gateway
git submodule add https://github.com/vshengur/bookings.monitoring.git services/monitoring
git submodule add https://github.com/vshengur/bookings.booking-frontend.git frontend/booking-frontend
git submodule add https://github.com/vshengur/bookings.payment-frontend.git frontend/payment-frontend
git submodule add https://github.com/vshengur/bookings.notification-frontend.git frontend/notification-frontend

git submodule add https://github.com/vshengur/bookings.consul.git services/consul
git submodule add https://github.com/vshengur/bookings.room-service.git services/room-service

Create docker compose network:
docker network create booking-net

Создайте пространство имен:
kubectl apply -f kubernetes/namespace.yaml

Minikube create tunnel to kubernetes:
minikube tunnel

Ingress Controller
Если у вас ещё не установлен Ingress Controller (например, NGINX), установите его. Пример для NGINX:
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/main/deploy/static/provider/cloud/deploy.yaml

Install helm charts - https://helm.sh/docs/intro/install/

Follow this instruction - https://cert-manager.io/docs/tutorials/acme/nginx-ingress/


Проблема: Ingress Controller не обрабатывает запросы

    Убедитесь, что аннотации для NGINX корректны:

annotations:
    nginx.ingress.kubernetes.io/rewrite-target: /

Перезапустите Ingress Controller:
    kubectl rollout restart deployment ingress-nginx-controller -n ingress-nginx