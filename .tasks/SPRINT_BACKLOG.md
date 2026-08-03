# Sprint Backlog — Фаза 1: MVP

**Дата создания:** 2026-02-21
**Цель:** Рабочий end-to-end флоу: поиск номера → бронирование → оплата → подтверждение

---

## Карта зависимостей

```
                    ┌──────────────┐
                    │   GW-001     │  API Gateway routing
                    │   (P0)       │  (разблокирует frontend)
                    └──────┬───────┘
                           │
            ┌──────────────┼───────────────┐
            ▼              ▼               ▼
     ┌────────────┐ ┌────────────┐ ┌──────────────┐
     │ BOOK-001   │ │ PAY-001    │ │  FRONT-001   │
     │ confirm/   │ │ configurable│ │  monorepo    │
     │ cancel     │ │ amount     │ │  init        │
     │ (P0)       │ │ (P0)       │ │  (P1)        │
     └─────┬──────┘ └─────┬──────┘ └──────┬───────┘
           │              │               │
     ┌─────▼──────┐ ┌─────▼──────┐ ┌──────▼───────┐
     │ BOOK-002   │ │ PAY-002    │ │  FRONT-002   │
     │ room-svc   │ │ webhook    │ │  search &    │
     │ integration│ │ security   │ │  browse      │
     │ (P0)       │ │ (P0)       │ │  (P1)        │
     └─────┬──────┘ └─────┬──────┘ └──────────────┘
           │              │
     ┌─────▼──────┐ ┌─────▼──────┐
     │ BOOK-003   │ │ PAY-003    │
     │ list       │ │ typed      │
     │ bookings   │ │ contracts  │
     │ (P0)       │ │ (P1)       │
     └─────┬──────┘ └────────────┘
           │
     ┌─────▼──────┐
     │ BOOK-004   │
     │ validation │
     │ (P1)       │
     └────────────┘

  Независимые:
  ┌──────────────┐  ┌──────────────┐
  │ GUEST-001    │  │ NOTIFY-001   │
  │ guest svc    │  │ notif svc    │
  │ (P1)         │  │ (P1)         │
  └──────────────┘  └──────┬───────┘
                           │ зависит от
                           │ GUEST-001 (email)
```

---

## Backlog — полная таблица

| # | Задача | Название | Priority | Зависит от | Блокирует | Статус | ETA |
|---|--------|----------|----------|------------|-----------|--------|-----|
| 1 | **GW-001** | API Gateway — routing для всех сервисов | **P0** | — | FRONT-002 | Open | 2-3 дня |
| 2 | **BOOK-001** | Booking confirm/cancel endpoints | **P0** | — | BOOK-002, BOOK-003 | Open | 2-3 дня |
| 3 | **PAY-001** | Убрать hardcoded values из Payment Service | **P0** | — | PAY-002 | ✅ Accepted | 2 дня |
| 4 | **PAY-002** | Webhook HMAC security (CRITICAL) | **P0** | PAY-001 | PAY-003 | Open | 2-3 дня |
| 5 | **BOOK-002** | Интеграция с Room Service (gRPC) | **P0** | BOOK-001 | BOOK-007 | Open | 3-4 дня |
| 6 | **BOOK-003** | List bookings endpoint + pagination | **P0** | BOOK-001 | — | Open | 1-2 дня |
| 7 | **BOOK-004** | Input validation при создании | **P1** | — | — | Open | 1-2 дня |
| 8 | **PAY-003** | Типизированные MassTransit контракты | **P1** | PAY-002 | — | Open | 1 день |
| 9 | **GUEST-001** | Создать Guest Service | **P1** | — | NOTIFY-001 | Open | 3-4 дня |
| 10 | **NOTIFY-001** | Notification Service (email) | **P1** | GUEST-001 | — | Open | 3-4 дня |
| 11 | **FRONT-001** | Frontend монорепо init | **P1** | — | FRONT-002 | Open | 2-3 дня |
| 12 | **FRONT-002** | Search & Browse pages | **P1** | FRONT-001, GW-001 | — | Open | 4-5 дней |
| 13 | **BOOK-005** | Промокоды | **P2** | BOOK-004 | — | Open | 2-3 дня |
| 14 | **BOOK-006** | Pricing Service интеграция | **P2** | BOOK-002 | — | Open | 2-3 дня |
| 15 | **BOOK-007** | Booking timeout handling (e2e) | **P2** | BOOK-002 | — | Open | 1-2 дня |
| 16 | **PAY-004** | Refund improvements | **P2** | PAY-001 | — | Open | 2-3 дня |
|| | | | | | | |
| **INFRA** | | | | | | | |
| 17 | **INFRA-001** | Единый Docker Compose | **P0** | — | INFRA-002 | Open | 4-6 часов |
| 18 | **INFRA-002** | Root Makefile + dev workflow | **P0** | INFRA-001 | — | Open | 2-3 часа |
| 19 | **INFRA-003** | Dockerfile optimization | **P1** | — | INFRA-004, INFRA-005 | Open | 3-4 часа |
| 20 | **INFRA-004** | K8s-манифесты для всех сервисов | **P1** | INFRA-003 (желат.) | INFRA-006 | Open | 6-8 часов |
| 21 | **INFRA-005** | CI/CD Pipeline (GitHub Actions) | **P1** | INFRA-003 (желат.) | — | Open | 4-5 часов |
| 22 | **INFRA-006** | Helm Charts для app-сервисов | **P2** | INFRA-004 | — | Open | 5-6 часов |

---

## Рекомендуемый порядок выполнения

### Волна 1 (параллельно, нет зависимостей)
Задачи, которые можно начинать немедленно:

| Поток | Задача | Оценка | Статус |
|-------|--------|--------|--------|
| Backend #1 | **GW-001** — API Gateway routing | 2-3 дня | Open |
| Backend #2 | **BOOK-001** — Booking confirm/cancel | 2-3 дня | Open |
| Backend #3 | ~~**PAY-001** — Configurable payment amount~~ | ~~1-2 дня~~ | ✅ Done |
| Backend #4 | **BOOK-004** — Input validation | 1-2 дня | Open |
| Frontend | **FRONT-001** — Monorepo init | 2-3 дня | Open |
| Backend #5 | **GUEST-001** — Guest Service (независимый) | 3-4 дня | Open |
| **DevOps** | **INFRA-001** — Единый Docker Compose | 4-6 часов | Open |
| **DevOps** | **INFRA-003** — Dockerfile optimization | 3-4 часа | Open |

### Волна 2 (после завершения зависимостей из Волны 1)

| Поток | Задача | Ждёт |
|-------|--------|------|
| Backend #1 | **PAY-002** — Webhook HMAC security | PAY-001 ✅ |
| Backend #2 | **BOOK-002** — Room Service integration | BOOK-001 |
| Backend #3 | **BOOK-003** — List bookings | BOOK-001 |
| Frontend | **FRONT-002** — Search & Browse pages | FRONT-001 + GW-001 |
| **DevOps** | **INFRA-002** — Root Makefile + dev workflow | INFRA-001 |
| **DevOps** | **INFRA-004** — K8s-манифесты для всех сервисов | INFRA-003 (желат.) |
| **DevOps** | **INFRA-005** — CI/CD Pipeline (GitHub Actions) | INFRA-003 (желат.) |

### Волна 3

| Поток | Задача | Ждёт |
|-------|--------|------|
| Backend #1 | **PAY-003** — Typed MassTransit contracts | PAY-002 |
| Backend #2 | **NOTIFY-001** — Notification Service | GUEST-001 |
| Backend #3 | **BOOK-007** — Booking timeout (e2e verify) | BOOK-002 |
| Backend #4 | **BOOK-006** — Pricing integration | BOOK-002 |

### Волна 4 (P2, можно отложить)

| Задача | Ждёт |
|--------|------|
| **BOOK-005** — Промокоды | BOOK-004 |
| **PAY-004** — Refund improvements | PAY-001 ✅ |
| **INFRA-006** — Helm Charts для app-сервисов | INFRA-004 |

---

## Риски

| Риск | Вероятность | Impact | Митигация |
|------|-------------|--------|-----------|
| gRPC-контракт room-service несовместим с booking-service | Средняя | High | Проверить proto/room.proto до начала BOOK-002 |
| MassTransit namespace mismatch между сервисами | Высокая | Critical | PAY-003 в приоритете; e2e тест Saga |
| Stripe webhook secret не настроен для dev env | Низкая | Medium | Использовать Stripe CLI для local webhook testing |
| Frontend mock data расходится с реальным API | Средняя | Medium | Types package (@bookings/types) как единый контракт |
| Guest Service не готов для Notification Service | Средняя | Medium | Notification может брать email из payload события (fallback) |

---

## Метрики MVP

| Метрика | Целевое значение |
|---------|-----------------|
| Все P0 задачи завершены | 6/6 |
| Все P1 задачи завершены | 6/6 |
| Happy path работает e2e | ✅ |
| Test coverage (бизнес-логика) | ≥ 70% |
| API response time (p95) | < 200ms |
| Payment flow (e2e) | < 3s |
| Zero Critical security issues | ✅ |

---

**Документ актуален на:** 2026-02-22 (добавлены INFRA-001..006, PAY-001 accepted)
**Обновлять:** при каждом изменении статуса задачи
