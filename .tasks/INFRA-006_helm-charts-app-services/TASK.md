# INFRA-006: Helm Charts для application-сервисов

## Статус: Open

## Описание
Существует только Helm chart для инфраструктуры (`infra/helm/booking-infra/`). Для application-сервисов нет параметризованных charts. Необходимо создать Helm charts для всех сервисов с поддержкой разных окружений (dev/staging/prod) и управляемыми values.

## Acceptance Criteria
- [ ] AC-1: Helm chart template для Go-сервисов (переиспользуемый)
- [ ] AC-2: Helm chart template для C#-сервисов (переиспользуемый)
- [ ] AC-3: Per-service values: image tag, replicas, resources, env vars, config
- [ ] AC-4: `helm install booking-auth ./charts/auth-service -f values-dev.yaml` — деплоит auth-service
- [ ] AC-5: `helm upgrade --install` — idempotent, обновляет существующий release
- [ ] AC-6: Values per environment: `values-dev.yaml`, `values-staging.yaml`, `values-prod.yaml`
- [ ] AC-7: Chart dependencies: сервис зависит от infra (postgres, rabbitmq)
- [ ] AC-8: `helm test` — smoke test для каждого chart (проверка health endpoint)

## Функциональные требования
- FR-1: Структура: `kubernetes/charts/{service-name}/` (Chart.yaml, values.yaml, templates/)
- FR-2: Доработать `infra/helm/booking-infra/` — values для разных окружений
- FR-3: Umbrella chart: `kubernetes/charts/booking-system/` — деплоит все сервисы + инфраструктуру через dependencies
- FR-4: Templates: deployment, service, configmap, secret, ingress, hpa (optional), serviceaccount
- FR-5: Helpers: common labels, selectors, fullname в `_helpers.tpl`

## Out of Scope
- ArgoCD ApplicationSet (отдельная задача)
- Helm hooks для миграций (Phase 2)
- Chart museum / OCI registry для charts

## Dependencies
- INFRA-004 (K8s-манифесты) — Helm charts параметризуют те же манифесты
- INFRA-003 (Dockerfile) — image references в values

## Priority: P2

## Assignee: DevOps-инженер

## Estimated Time: 5-6 часов

## Связь с Roadmap
- IMPLEMENTATION_ROADMAP.md: секция 12.1 (Kubernetes Deployment)
