# INFRA-002: Root Makefile и dev-workflow

## Статус: Open

## Описание
Разработчику сейчас нужно помнить множество команд для разных задач (запуск compose, миграции, тесты, сборка). Необходимо создать единый Makefile (или Taskfile) в корне проекта, который стандартизирует dev-workflow и упрощает ежедневную работу.

## Acceptance Criteria
- [ ] AC-1: `make up` — запускает всю систему (или `make up-infra`, `make up-backend`)
- [ ] AC-2: `make down` — останавливает систему, `make clean` — останавливает + удаляет volumes
- [ ] AC-3: `make build [service]` — собирает Docker image конкретного сервиса
- [ ] AC-4: `make test [service]` — запускает тесты сервиса (unit + integration)
- [ ] AC-5: `make logs [service]` — логи конкретного сервиса (follow mode)
- [ ] AC-6: `make migrate [service]` — применяет миграции БД для сервиса
- [ ] AC-7: `make status` — показывает статус всех контейнеров
- [ ] AC-8: `make help` — описание всех доступных команд
- [ ] AC-9: `make seed` — заполняет БД тестовыми данными (rooms, pricing rules)

## Функциональные требования
- FR-1: Makefile в корне проекта (не в infra/)
- FR-2: Команды для всего lifecycle: build, up, down, restart, logs, test, migrate, seed, clean, status
- FR-3: Поддержка аргументов: `make logs service=payment-service`, `make test service=bookings-service`
- FR-4: Цветной вывод help с группировкой команд
- FR-5: Seed data: использовать существующие SQL из room-service (005_seed_data.sql) + создать seed для других сервисов

## Нефункциональные требования
- NFR-1: Makefile должен работать на Windows (через Make for Windows / WSL), macOS, Linux
- NFR-2: Fallback: если make не установлен — документация по прямым docker compose командам

## Edge Cases / Corner Cases
- EC-1: `make test` когда контейнеры не запущены — понятное сообщение об ошибке
- EC-2: `make build` с ошибкой компиляции — не падает молча, показывает ошибку
- EC-3: `make seed` повторный запуск — idempotent (не дублирует данные)

## Out of Scope
- CI/CD интеграция (INFRA-005)
- K8s-специфичные команды (INFRA-004)

## Dependencies
- INFRA-001 (единый docker-compose) — Makefile использует его

## Priority: P0

## Assignee: DevOps-инженер

## Estimated Time: 2-3 часа

## Связь с Roadmap
- IMPLEMENTATION_ROADMAP.md: секция 12, разработка dev-tooling
