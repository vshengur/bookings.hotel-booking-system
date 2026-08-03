# GUEST-001: Создать базовый Guest Service

## Статус: Open

## Описание
Для полноценного бронирования нужен профиль гостя: имя, email, телефон, адрес. Сейчас bookings-service использует только GuestId (GUID) без реальных данных гостя. Фронтенд-страница Profile и форма бронирования (guest info) требуют CRUD API для управления профилем.

## Acceptance Criteria
- [ ] AC-1: Сервис guest-service запускается, доступен по HTTP и gRPC
- [ ] AC-2: `POST /api/guests` — создание профиля гостя (при первом входе через OAuth)
- [ ] AC-3: `GET /api/guests/me` — получение профиля текущего пользователя (по userId из JWT)
- [ ] AC-4: `PUT /api/guests/me` — обновление профиля
- [ ] AC-5: `GET /api/guests/{id}` — получение профиля по ID (для admin и inter-service calls)
- [ ] AC-6: Сервис зарегистрирован в Consul с health check
- [ ] AC-7: PostgreSQL database с миграциями
- [ ] AC-8: Prometheus metrics endpoint `/metrics`
- [ ] AC-9: Dockerfile для сборки и деплоя
- [ ] AC-10: Покрыто unit-тестами для handlers

## Функциональные требования
- FR-1: Технология: Go (Gin, GORM) — consistency с auth-service и room-service
- FR-2: Модель Guest: Id, UserId (from OAuth), FirstName, LastName, Email, Phone, DateOfBirth, Address (street, city, country, postalCode), CreatedAt, UpdatedAt
- FR-3: gRPC сервис для inter-service calls (booking-service запрашивает email гостя)
- FR-4: Input validation: email format, phone format (E.164), required fields
- FR-5: UserId уникален — один профиль на пользователя

## Нефункциональные требования
- NFR-1: Время ответа < 100ms (p95)
- NFR-2: Данные шифруются at rest (PostgreSQL encryption)

## Edge Cases / Corner Cases
- EC-1: Повторное создание профиля для того же userId → 409 Conflict
- EC-2: GET /api/guests/me без профиля → 404 (ещё не создан)
- EC-3: PUT с пустыми required fields → 400
- EC-4: Очень длинные значения (name > 200 chars) → 400

## Out of Scope
- GDPR compliance (data export, deletion) — фаза 2
- Loyalty points
- Guest history (booking history)

## Dependencies
- Auth Service — userId берётся из JWT
- Consul — для service discovery

## Priority: P1

## Assignee: Кодер

## Связь с Roadmap
- IMPLEMENTATION_ROADMAP.md: секция 7.1, 7.2, 7.3
- FRONTEND_ROADMAP.md: экран 8 "User Profile"
