# INFRA-005: CI/CD Pipeline (GitHub Actions)

## Статус: Open

## Описание
В проекте полностью отсутствует CI/CD. Код мержится в main без автоматической проверки build/test. Необходимо создать GitHub Actions pipelines для: 1) CI — build + test на каждый PR, 2) CD — build Docker images и push в registry при мерже в main.

## Acceptance Criteria
- [ ] AC-1: При создании/обновлении PR: автоматически собираются и тестируются все затронутые сервисы
- [ ] AC-2: Go-сервисы: `go build`, `go test`, `golangci-lint run`
- [ ] AC-3: C#-сервисы: `dotnet build`, `dotnet test`
- [ ] AC-4: При мерже в main: Docker images собираются и пушатся в GitHub Container Registry (ghcr.io)
- [ ] AC-5: Image tags: `{service}:{git-sha-short}`, `{service}:latest`
- [ ] AC-6: CI fail блокирует мерж PR (status check required)
- [ ] AC-7: Среднее время CI pipeline < 5 минут
- [ ] AC-8: Кэширование зависимостей (Go modules, NuGet packages) между запусками

## Функциональные требования
- FR-1: `.github/workflows/ci.yml` — PR checks (build + test + lint)
- FR-2: `.github/workflows/docker-publish.yml` — build + push images при мерже в main
- FR-3: Matrix strategy: параллельная сборка Go-сервисов и C#-сервисов
- FR-4: Path filters: CI запускается только для затронутых сервисов (changes in `services/auth-service/` → build только auth-service)
- FR-5: Docker image naming: `ghcr.io/{owner}/booking-{service}:{tag}`
- FR-6: Secrets: GHCR token (автоматический), Stripe test key (для integration tests)

## Нефункциональные требования
- NFR-1: CI pipeline < 5 минут (с кэшированием)
- NFR-2: Docker build + push < 3 минуты per service
- NFR-3: Минимальное потребление Actions minutes

## Edge Cases / Corner Cases
- EC-1: PR затрагивает только docs (.md файлы) → CI не запускается
- EC-2: PR затрагивает common/ (shared contracts) → rebuild all dependent services
- EC-3: Flaky tests → retry strategy (max 2 retries)
- EC-4: GHCR rate limits → proper auth и caching

## Out of Scope
- Deploy на staging/production (Phase 2, потребует K8s cluster)
- ArgoCD integration
- Security scanning (SAST/DAST) — отдельная задача
- Frontend CI (нет приложений)

## Dependencies
- INFRA-003 (оптимизированные Dockerfile) — желательно для быстрого build
- GitHub repository settings: branch protection rules для main

## Priority: P1

## Assignee: DevOps-инженер

## Estimated Time: 4-5 часов

## Связь с Roadmap
- IMPLEMENTATION_ROADMAP.md: секция 12.2 (CI/CD Pipeline)
