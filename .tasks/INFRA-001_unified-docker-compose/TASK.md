# INFRA-001: Единый Docker Compose для всей системы

## Статус: In Progress

## Описание
Сейчас в проекте 17 отдельных docker-compose файлов, разбросанных по сервисам. Нет способа запустить всю систему одной командой. Это замедляет разработку, затрудняет integration-тестирование и onboarding новых разработчиков. Необходимо создать единый `docker-compose.yml` в корне проекта с поддержкой profiles для запуска подмножеств.

## Acceptance Criteria
- [ ] AC-1: `docker compose up` из корня проекта поднимает ВСЮ систему (инфра + все сервисы)
- [ ] AC-2: Profiles позволяют запускать подмножества: `docker compose --profile infra up` (только инфра), `docker compose --profile backend up` (инфра + backend), `docker compose --profile full up` (всё)
- [ ] AC-3: Все сервисы доступны и взаимодействуют друг с другом (Consul registration, RabbitMQ messaging, DB connectivity)
- [ ] AC-4: Health checks для всех сервисов — compose ждёт зависимости (`depends_on` с `condition: service_healthy`)
- [ ] AC-5: `.env.example` в корне с описанием всех переменных и dev-значениями по умолчанию
- [ ] AC-6: Единая сеть `booking-net` для всех сервисов
- [ ] AC-7: Volumes для персистентных данных (PostgreSQL, RabbitMQ, Elasticsearch)
- [ ] AC-8: README или секция в существующем README: как запустить, prerequisite, troubleshooting

## Функциональные требования
- FR-1: Сервисы в compose: PostgreSQL (с инициализацией нескольких БД — auth, bookings, payments, rooms, pricing), RabbitMQ, Consul, Elasticsearch, Kibana, Seq, Prometheus, Grafana, MongoDB
- FR-2: Application-сервисы: auth-service, room-service, bookings-service, payment-service, pricing-service, api-gateway (выбрать один — Go или C#)
- FR-3: Dev-gateway (nginx) как единая точка входа на localhost:8080
- FR-4: PostgreSQL init script для создания нескольких БД из одного инстанса (или отдельные инстансы — обосновать решение)
- FR-5: Consul init container для загрузки KV-конфигурации
- FR-6: Порядок запуска: infra → consul-init → application services → gateway

## Нефункциональные требования
- NFR-1: `docker compose up` должен стартовать за < 3 минут на машине с 16GB RAM
- NFR-2: Суммарное потребление RAM всех контейнеров < 8GB
- NFR-3: Compose файл должен работать на Windows (Docker Desktop), macOS, Linux

## Edge Cases / Corner Cases
- EC-1: Повторный `docker compose up` при уже запущенных контейнерах — не должен дублировать
- EC-2: `docker compose down -v` — полная очистка, следующий up создаёт всё заново
- EC-3: Один из сервисов не собирается (ошибка компиляции) — остальные должны подняться
- EC-4: PostgreSQL init: повторный запуск не должен дублировать БД

## Out of Scope
- Frontend-сервисы (пока нет приложений)
- Production-ready конфигурация (это dev-окружение)
- TLS/HTTPS для локальной разработки
- ArgoCD, Helm (отдельные задачи)

## Dependencies
- Существующие Dockerfile в каждом сервисе (могут потребовать мелкие правки)
- `infra/docker-compose.infra.yml` — использовать как базу для инфра-части
- `services/dev-gateway/` — использовать как базу для nginx gateway

## Priority: P0

## Assignee: DevOps-инженер

## Estimated Time: 4-6 часов

## Связь с Roadmap
- IMPLEMENTATION_ROADMAP.md: секция 12 (Infrastructure & DevOps), Docker Compose setup

## Ключевые файлы (текущие)
- `infra/docker-compose.infra.yml` — текущий инфра compose
- `infra/.env`, `infra/.env.example` — переменные окружения
- `services/dev-gateway/docker-compose.yml` — nginx gateway
- `services/*/Dockerfile` — Dockerfile каждого сервиса
- `services/*/docker-compose.yml` — per-service compose файлы (будут заменены)
