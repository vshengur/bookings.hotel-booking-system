# INFRA-004: K8s-манифесты для всех сервисов

## Статус: Open

## Описание
Kubernetes-манифесты существуют только для api-gateway, auth-service и consul. Отсутствуют для: bookings-service, payment-service, pricing-service, room-service. Необходимо создать полный набор манифестов для всех сервисов и организовать их через Kustomize (base + overlays для dev/staging/prod).

## Acceptance Criteria
- [ ] AC-1: K8s-манифесты для каждого сервиса: Deployment, Service, ConfigMap, Secret (template)
- [ ] AC-2: Liveness и Readiness probes для каждого Deployment
- [ ] AC-3: Resource limits/requests для каждого контейнера
- [ ] AC-4: Kustomize structure: `kubernetes/base/`, `kubernetes/overlays/dev/`, `kubernetes/overlays/staging/`
- [ ] AC-5: `kubectl apply -k kubernetes/overlays/dev/` — деплоит всю систему в локальный K8s кластер
- [ ] AC-6: Ingress rules: единая точка входа с routing на сервисы
- [ ] AC-7: Скрипт для инициализации локального кластера (minikube/kind) с необходимыми addons
- [ ] AC-8: Документация: как запустить систему в K8s локально

## Функциональные требования
- FR-1: Манифесты для сервисов: auth-service, room-service, bookings-service, payment-service, pricing-service, api-gateway
- FR-2: Манифесты для инфраструктуры: PostgreSQL (StatefulSet), RabbitMQ, Consul, Seq
- FR-3: ConfigMaps: appsettings per service, env-specific values
- FR-4: Secrets: DB credentials, RabbitMQ creds, Stripe API key, JWT secret — sealed-secrets или .example templates
- FR-5: Init containers: wait-for-db, consul-init
- FR-6: Namespace: `booking-system` (dev), `booking-system-staging`, `booking-system-prod`
- FR-7: NetworkPolicy: сервисы могут обращаться только к нужным зависимостям

## Нефункциональные требования
- NFR-1: Все манифесты проходят `kubectl apply --dry-run=client` без ошибок
- NFR-2: Kube-score >= 7/10 для каждого манифеста
- NFR-3: Система поднимается в minikube с 4 CPU, 8GB RAM

## Edge Cases / Corner Cases
- EC-1: Pod restarts при OOM — правильные resource limits
- EC-2: Rolling update: zero downtime при обновлении Deployment
- EC-3: PostgreSQL data persistence: PVC не удаляется при `kubectl delete deployment`

## Out of Scope
- Production-grade HA (multi-replica, pod disruption budgets)
- ArgoCD integration (INFRA-006+)
- Helm charts (INFRA-006) — здесь raw manifests + Kustomize
- HPA (HorizontalPodAutoscaler) — Phase 2

## Dependencies
- INFRA-003 (оптимизированные Dockerfile) — желательно, но не блокирует
- Существующие K8s-манифесты в `services/api-gateway/kubernetes/`, `services/auth-service/kubernetes/`, `services/consul/kubernetes/`

## Priority: P1

## Assignee: DevOps-инженер

## Estimated Time: 6-8 часов

## Связь с Roadmap
- IMPLEMENTATION_ROADMAP.md: секция 12.1 (Kubernetes Deployment)
