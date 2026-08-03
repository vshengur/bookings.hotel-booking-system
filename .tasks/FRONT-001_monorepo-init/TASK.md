# FRONT-001: Инициализация frontend монорепо

## Статус: Open

## Описание
Frontend-часть проекта состоит из пустых placeholder-репозиториев. Для начала разработки необходимо инициализировать монорепо с pnpm workspaces, Turborepo, настроить TypeScript, ESLint, Prettier, создать структуру проектов (apps/ и packages/) и инициализировать Guest Frontend (Next.js 15).

## Acceptance Criteria
- [ ] AC-1: Монорепо инициализировано: pnpm-workspace.yaml, turbo.json, корневой package.json
- [ ] AC-2: `apps/guest-frontend/` — инициализирован Next.js 15 с App Router, TypeScript, Tailwind CSS
- [ ] AC-3: `packages/types/` — пакет @bookings/types с базовыми TypeScript definitions (Room, Booking, Guest, Payment)
- [ ] AC-4: `packages/api-client/` — пакет @bookings/api-client с Axios instance и interceptors
- [ ] AC-5: Базовый tsconfig.json в packages/config/ с strict mode
- [ ] AC-6: ESLint + Prettier настроены с pre-commit hooks (husky + lint-staged)
- [ ] AC-7: `pnpm dev` запускает guest-frontend на localhost:3000
- [ ] AC-8: `pnpm build` собирает все packages и apps без ошибок
- [ ] AC-9: `pnpm lint` и `pnpm type-check` проходят без ошибок
- [ ] AC-10: `.env.example` для guest-frontend с переменными API_URL, WS_URL, STRIPE_PK
- [ ] AC-11: Docker-compose.dev.yml для frontend-dev окружения

## Функциональные требования
- FR-1: Структура монорепо по FRONTEND_ROADMAP.md (Phase 0)
- FR-2: Guest Frontend: Next.js 15 + React 19 + Tailwind + Shadcn/ui init
- FR-3: @bookings/types: interfaces для Room, Booking, BookingStatus, GuestInfo, Payment, SearchParams
- FR-4: @bookings/api-client: базовый ApiClient с JWT interceptor и error handling

## Нефункциональные требования
- NFR-1: pnpm install < 60s (clean install)
- NFR-2: Build < 30s (all packages + app)
- NFR-3: TypeScript strict mode без any

## Edge Cases / Corner Cases
- Нет (инфраструктурная задача)

## Out of Scope
- Страницы и компоненты Guest Frontend (FRONT-002)
- Payment Frontend, Admin Frontend, Notification Widget
- @bookings/auth-sdk, @bookings/websocket (позже)

## Dependencies
- Node.js 22, pnpm 9+

## Priority: P1

## Assignee: Кодер

## Связь с Roadmap
- FRONTEND_ROADMAP.md: Phase 0 "Подготовка инфраструктуры" и Phase 1 "Shared Packages" (частично)
