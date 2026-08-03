# Hotel Booking System - Implementation Roadmap

## Дата создания: 2026-01-18

---

## User Case: Основной флоу бронирования номера

### Описание сценария использования

**Актор:** Гость отеля (пользователь системы)

**Цель:** Забронировать номер в отеле с оплатой онлайн

**Предусловия:**
- Пользователь имеет доступ к системе
- В системе есть доступные номера

### Основной сценарий (Happy Path)

1. **Аутентификация**
   - Пользователь заходит в систему
   - Система аутентифицирует пользователя через Google OAuth
   - Пользователь получает JWT токен

2. **Поиск номеров**
   - Пользователь указывает даты заезда и выезда
   - Пользователь указывает количество гостей
   - Система показывает список доступных номеров
   - Пользователь фильтрует по цене, удобствам, вместимости

3. **Просмотр деталей номера**
   - Пользователь выбирает интересующий номер
   - Система показывает детальную информацию: фото, описание, удобства, цену
   - Система подтверждает доступность на выбранные даты

4. **Создание бронирования**
   - Пользователь нажимает "Забронировать"
   - Пользователь вводит данные гостей (имя, email, телефон)
   - Пользователь может ввести промокод
   - Система рассчитывает итоговую стоимость
   - Система создает предварительное бронирование (статус: Created)
   - Система резервирует номер на 15 минут

5. **Оплата**
   - Система перенаправляет на страницу оплаты
   - Пользователь вводит данные карты
   - Payment Provider (Stripe) обрабатывает платеж
   - При успехе: статус меняется на AwaitingPayment → Reserved
   - Система получает webhook подтверждение

6. **Подтверждение в PMS**
   - Система отправляет данные бронирования в PMS (Property Management System)
   - PMS подтверждает бронирование и выдает confirmation number
   - Статус меняется на Confirmed

7. **Уведомление**
   - Система отправляет email с подтверждением бронирования
   - Email содержит: номер бронирования, детали номера, даты, сумму, инструкции для заезда

8. **Результат**
   - Пользователь получает подтвержденное бронирование
   - Номер зарезервирован на указанные даты
   - Платеж проведен

### Альтернативные сценарии

**A1. Платеж не прошел**
- После шага 5: Payment Provider отклоняет платеж
- Система отправляет PaymentFailed event
- Статус бронирования: Failed
- Резервирование номера отменяется
- Пользователь получает уведомление об ошибке
- Пользователь может попробовать снова

**A2. Timeout оплаты**
- После шага 4: прошло 15 минут без оплаты
- Saga переводит бронирование в статус Expired
- Резервирование номера отменяется
- Пользователь получает уведомление

**A3. PMS не подтвердила**
- После шага 6: PMS не отвечает или отклоняет
- Saga обрабатывает ошибку
- Необходим manual review или автоматический refund
- Пользователь получает уведомление

**A4. Отмена бронирования**
- Пользователь отменяет бронирование
- Система обрабатывает возврат средств (по политике отмены)
- Резервирование номера отменяется
- Статус: Cancelled

---

## Текущий статус реализации

### Легенда
- ✅ **РЕАЛИЗОВАНО** - компонент полностью работает
- ⚠️ **ЧАСТИЧНО** - компонент существует, но неполный/с заглушками
- ❌ **ОТСУТСТВУЕТ** - компонент не реализован
- 🔧 **В РАБОТЕ** - компонент в процессе разработки

---

## Компоненты системы

### 1. Аутентификация (Authentication)
**Статус:** ✅ **РЕАЛИЗОВАНО**

**Сервис:** `services/auth-service` (Go)

**Реализовано:**
- ✅ Google OAuth 2.0 integration
- ✅ JWT token generation и validation
- ✅ Endpoints:
  - `GET /login` - инициация OAuth flow
  - `GET /callback` - OAuth callback
  - `POST /validate-token` - валидация токена
  - CRUD для пользователей: GET, POST, PUT, DELETE `/users`
- ✅ PostgreSQL database для хранения пользователей
- ✅ Service registration в Consul
- ✅ Health checks

**Файлы:**
- [main.go](services/auth-service/cmd/main.go)
- [handlers/auth.go](services/auth-service/handlers/auth.go)
- [services/auth_service.go](services/auth-service/services/auth_service.go)
- [local.env](services/auth-service/local.env)

**Нет задач для этого компонента**

---

### 2. Поиск и каталог номеров (Room Search & Catalog)
**Статус:** ✅ **РЕАЛИЗОВАНО**

**Сервис:** `services/room-service` (Go)

**Реализовано:**
- ✅ Room Service (Go, Gin, GORM)
- ✅ PostgreSQL, миграции, модели: Room, RoomAmenity, RoomImage, RoomAvailability
- ✅ CRUD API: `GET/POST /api/rooms`, `GET/PUT/DELETE /api/rooms/:id`
- ✅ Search API: `GET /api/rooms/search` (checkIn, checkOut, adults, children, minPrice, maxPrice, roomType, amenities, pagination, sortBy)
- ✅ Availability API: `GET /api/rooms/:id/availability`, `POST /api/rooms/:id/reserve`, `POST /api/rooms/:id/release`
- ✅ gRPC (proto/room.proto), Consul, health checks, Prometheus metrics
- ✅ Seed data (migrations/005_seed_data.sql)

**Файлы:**
- [main.go](services/room-service/cmd/server/main.go)
- [room_handler.go](services/room-service/internal/handlers/room_handler.go)
- [room_service.go](services/room-service/internal/services/room_service.go)
- [README.md](services/room-service/README.md)

#### Оставшиеся задачи:

- [ ] **2.8. Интеграция с API Gateway**
  - [ ] Добавить routing в gateway для `/api/rooms/**` → Room Service
  - [ ] JWT authentication для admin endpoints (POST/PUT/DELETE rooms)
  - [ ] Rate limiting

---

### 2.5. Сервис цен (Pricing Service)
**Статус:** ✅ **РЕАЛИЗОВАНО**

**Сервис:** `services/pricing-service` (C#/.NET)

**Реализовано:**
- ✅ REST API: расчёт цен `POST /api/pricing/calculate`, CRUD для room prices и pricing rules
- ✅ gRPC: CalculatePrice, GetRoomPrice, GetAllActivePrices
- ✅ Стратегии: Base, Seasonal, Weekend, Promotional, Dynamic
- ✅ PostgreSQL, EF Core, Consul, Prometheus, Docker
- ✅ Controller-level error handling перенесён в middleware

**Файлы:**
- [PricingController.cs](services/pricing-service/PricingService/Controllers/PricingController.cs)
- [PricingService.cs](services/pricing-service/PricingService.Application/Services/PricingService.cs)
- [README.md](services/pricing-service/README.md)

**Интеграция:** Room Service и Payment Service могут получать цены через gRPC/REST.

---

### 3. Управление бронированиями (Booking Management)
**Статус:** ⚠️ **ЧАСТИЧНО**

**Сервис:** `services/bookings-service` (C#/.NET)

**Реализовано:**
- ✅ `POST /api/booking` - создание бронирования
- ✅ `GET /api/booking/{id}` - получение бронирования
- ✅ `POST /api/booking/{id}/confirm` - больше не заглушка
- ✅ `POST /api/booking/{id}/cancel` - больше не заглушка
- ✅ Database: Booking, BookingLineItem entities
- ✅ MassTransit integration
- ✅ BookingStateMachine (Saga pattern)
  - States: Created, AwaitingPayment, Reserved, Confirmed, Cancelled, Expired, Failed
- ✅ Message publishing: CreateBooking, BookingCreated
- ✅ Message consuming: PaymentAuthorized, PaymentFailed, PmsConfirmed
- ✅ Базовые unit-тесты для confirm/cancel handlers
- ✅ Начата optimistic concurrency защита для booking updates
- ✅ Controller-level error handling перенесён в middleware
- ✅ `roomId` в bookings-service нормализован под `long`
- ✅ Реальный room-service-backed inventory gateway подключён в DI

**Файлы:**
- [BookingController.cs](services/bookings-service/src/Api/Controllers/BookingController.cs)
- [CreateBookingCommandHandler.cs](services/bookings-service/src/Application/Commands/CreateBookingCommandHandler.cs)
- [BookingStateMachine.cs](services/bookings-service/src/Application/Saga/BookingStateMachine.cs)
- [Booking.cs](services/bookings-service/src/Domain/Aggregates/Booking/Booking.cs)

#### Задачи:

- [ ] **3.1. Завершить Booking API endpoints**
  - [ ] Добавить `GET /api/bookings` - список бронирований пользователя
  - [ ] Добавить `PUT /api/booking/{id}` - редактирование (до подтверждения)
  - [ ] Добавить pagination и filtering

- [ ] **3.2. Интеграция с Room Service**
  - [x] Нормализовать тип идентификатора комнаты между сервисами (`Guid` vs `int64`)
  - [x] Заменить симулированный gateway на реальную реализацию (Room Service уже есть)
  - [x] Перевести reserve/release на `booking_reference`, чтобы не терять Guid booking id
  - [ ] gRPC клиент для Room Service (proto/room.proto)
  - [x] Метод `ReserveAsync()` - вызов `POST /api/rooms/{id}/reserve`
  - [x] Метод `ReleaseAsync()` - вызов `POST /api/rooms/{id}/release`
  - [ ] Проверка доступности перед созданием бронирования

- [ ] **3.3. Валидация при создании**
  - [x] Проверка валидности дат (checkIn < checkOut, checkIn >= today)
  - [x] Проверка что бронирование содержит хотя бы один номер
  - [x] Проверка положительной стоимости за ночь
  - [x] Базовая проверка что номера доступны на указанные даты через room-service pre-check
  - [ ] Проверка что гость существует
  - [ ] Валидация промокода

- [ ] **3.4. PromoCode обработка**
  - [ ] Создать таблицу PromoCodes
  - [ ] Валидация промокода (существует, активен, не истек)
  - [ ] Применение скидки к итоговой сумме
  - [ ] Логирование использования промокодов

- [ ] **3.5. Расчет стоимости**
  - [ ] Получение цен за ночь из Payment Service
  - [ ] Расчет налогов и сборов
  - [ ] Применение скидок и промокодов
  - [ ] Сохранение breakdown в booking

- [ ] **3.6. Booking timeout handling**
  - [ ] Schedule автоматическое истечение через 15 минут
  - [ ] Освобождение резерва при истечении
  - [ ] Уведомление пользователя

- [ ] **3.7. Concurrency hardening**
  - [ ] Проверить гонки `confirm` vs `cancel`
  - [ ] Проверить гонки user action vs saga transition
  - [ ] Убедиться, что side effects не дублируются при конфликтующих обновлениях

---

### 4. Обработка платежей (Payment Processing)
**Статус:** ⚠️ **ЧАСТИЧНО** (прогресс: 4.1 завершена)

**Сервисы:**
- `services/payment-service` (C#/.NET)
- `services/payment-service-go` (Go)

**Реализовано:**
- ✅ `POST /payment/intent` - создание payment intent (Stripe) — **обновлён в PAY-001**
- ✅ `POST /payment/refund/{bookingId}` - возврат средств
- ✅ `POST /payment/webhook` - webhook от Stripe
- ✅ Database: PriceListItem, PaymentIntent (+ EF Core миграция: Amount bigint, ProviderRef, CreatedAt, unique index на BookingId)
- ✅ gRPC service для inter-service communication
- ✅ MassTransit publishing: PaymentAuthorized, PaymentFailed, **PaymentIntentCreated** (typed messages)
- ✅ Конфигурация через IOptions<T>: PaymentSettings, PspSettings, RabbitMqSettings (PAY-001)
- ✅ FluentValidation для входных данных (PAY-001)
- ✅ Placeholder-first idempotency по BookingId (PAY-001)
- ✅ PSP error handling → 503 с retry hint (PAY-001)
- ✅ Controller-level error handling перенесён в middleware; create-intent orchestration вынесена в service
- ✅ Unit-тесты: 24 (validator, builder, parser) (PAY-001)
- ✅ Integration-тесты: 5 (WebApplicationFactory) — написаны, ожидают прогона в CI (PAY-001)

**Файлы:**
- [PaymentController.cs](services/payment-service/src/PaymentService.API/Controllers/PaymentController.cs)
- [PaymentGatewayGrpc.cs](services/payment-service/Infrastructure/Gateways/PaymentGatewayGrpc.cs)
- [main.go](services/payment-service-go/main.go)
- [PaymentSettings.cs](services/payment-service/src/PaymentService.Application/Configuration/PaymentSettings.cs) (PAY-001)
- [PspSettings.cs](services/payment-service/src/PaymentService.Application/Configuration/PspSettings.cs) (PAY-001)
- [CreatePaymentIntentRequest.cs](services/payment-service/src/PaymentService.Application/DTOs/CreatePaymentIntentRequest.cs) (PAY-001)
- [PspResponseParser.cs](services/payment-service/src/PaymentService.Application/Helpers/PspResponseParser.cs) (PAY-001)

#### Задачи:

- [x] **4.1. Завершить Payment Intent** ✅ *(PAY-001, 2026-02-22)*
  - [x] Убрать hardcoded amount (250 EUR) из `/payment/intent`
  - [x] Получать actual booking amount из запроса (DTO: `{ bookingId, amount, currency }`)
  - [x] Добавить параметр `bookingId` в PaymentIntent (+ unique constraint)
  - [x] Сохранять booking details в PaymentIntent (Amount long/cents, Currency, ProviderRef, CreatedAt)
  - [x] Правильная обработка return URL и cancel URL (из IOptions<PaymentSettings>)
  - [x] PSP API key из конфигурации (не hardcoded "demo-key")
  - [x] RabbitMQ credentials из конфигурации (не hardcoded "guest"/"guest")
  - [x] Idempotency: placeholder-first flow (pending → PSP → created/failed)
  - [x] Валидация: FluentValidation (amount > 0, ISO 4217 currency, valid GUID)
  - [x] PSP errors → 503 с retry hint
  - [x] Typed MassTransit messages (PaymentIntentCreated, PaymentStatusChanged)
  - **Техдолг из PAY-001:** PAY-003 (Status string→enum), ARCH-001 (дальше довести orchestration из API service в полноценный Application Service)

- [ ] **4.2. Webhook Security (КРИТИЧНО)**
  - [ ] Реализовать HMAC signature validation (TODO в PaymentController.cs:74)
  - [ ] Добавить idempotency для повторных webhook calls
  - [ ] Логирование всех webhook events в отдельную таблицу
  - [ ] Rate limiting для webhook endpoint

- [ ] **4.3. Улучшить Quote API**
  - [ ] Получать реальный email гостя (TODO в PaymentGatewayGrpc.cs:46)
  - [ ] Получать currency из конфигурации (TODO в PaymentGatewayGrpc.cs:49)
  - [ ] Добавить breakdown: subtotal, taxes, fees, discounts
  - [ ] Кеширование price list

- [ ] **4.4. Refund Processing**
  - [ ] Добавить параметр `amount` в refund interface (TODO в PaymentGatewayGrpc.cs:77)
  - [ ] Поддержка partial refunds
  - [ ] Cancellation policy (полный/частичный возврат в зависимости от даты)
  - [ ] Расчет комиссий при возврате
  - [ ] Асинхронная обработка возвратов

- [ ] **4.5. Retry Logic**
  - [ ] Retry для failed payments
  - [ ] Exponential backoff
  - [ ] Dead letter queue для permanently failed payments

- [ ] **4.6. Price List Management**
  - [ ] Admin API для управления ценами
  - [ ] Publish PriceListUpdated event через MassTransit (TODO в NightlyTariffJob.cs:34)
  - [ ] Сезонные тарифы
  - [ ] Dynamic pricing на основе occupancy

- [ ] **4.7. Multiple Payment Methods**
  - [ ] Credit/Debit cards (уже есть через Stripe)
  - [ ] Bank transfer
  - [ ] PayPal
  - [ ] Apple Pay / Google Pay

---

### 5. Интеграция с PMS (Property Management System)
**Статус:** ⚠️ **ЧАСТИЧНО** (только заглушка)

**Реализовано:**
- ✅ Interface `IPmsGateway`
- ✅ Симулированная реализация `PmsGatewaySimulated`
- ✅ Saga обработка: RequestPmsConfirmation → PmsConfirmed

**Файлы:**
- [IPmsGateway.cs](services/bookings-service/src/Domain/Interfaces/IPmsGateway.cs)
- [PmsGatewaySimulated.cs](services/bookings-service/src/Infrastructure/Gateways/PmsGatewaySimulated.cs)

#### Задачи:

- [ ] **5.1. Определить PMS систему**
  - [ ] Выбрать PMS (Opera, Mews, CloudBeds, custom)
  - [ ] Изучить API документацию
  - [ ] Получить API credentials

- [ ] **5.2. Создать PMS Gateway реализацию**
  - [ ] HTTP client для PMS API
  - [ ] Отправка booking details
  - [ ] Получение confirmation number
  - [ ] Синхронизация room status
  - [ ] Error handling и retry logic

- [ ] **5.3. Альтернатива: собственный PMS Service**
  - [ ] Создать `services/pms-service`
  - [ ] gRPC API для booking confirmation
  - [ ] Database для хранения PMS records
  - [ ] Интеграция с Booking Service

- [ ] **5.4. PMS Sync**
  - [ ] Синхронизация inventory
  - [ ] Синхронизация bookings
  - [ ] Обработка конфликтов
  - [ ] Scheduled sync jobs

- [ ] **5.5. Timeout handling**
  - [ ] Добавить timeout для PMS confirmation (аналогично payment)
  - [ ] Fallback при недоступности PMS
  - [ ] Manual review queue

---

### 6. Уведомления (Notifications)
**Статус:** ❌ **ОТСУТСТВУЕТ**

**Необходимо создать:** `services/notification-service`

#### Задачи:

- [ ] **6.1. Создать Notification Service (Go или C#)**
  - [ ] Инициализировать проект
  - [ ] Настроить database (PostgreSQL)
  - [ ] Models: NotificationTemplate, NotificationLog, UserPreferences
  - [ ] Service registration в Consul
  - [ ] Health checks

- [ ] **6.2. Email Integration**
  - [ ] Выбрать email provider (SendGrid, AWS SES, Mailgun)
  - [ ] Настроить SMTP или API клиент
  - [ ] HTML email templates
  - [ ] Templating engine (Handlebars, Liquid и т.д.)

- [ ] **6.3. Email Templates**
  - [ ] Booking Confirmation
  - [ ] Payment Receipt
  - [ ] Booking Cancellation
  - [ ] Check-in Reminder (за 24 часа)
  - [ ] Invoice/Receipt
  - [ ] Password Reset
  - [ ] Welcome Email

- [ ] **6.4. MassTransit Consumers**
  - [ ] Consumer для BookingCreated → Send confirmation
  - [ ] Consumer для PaymentAuthorized → Send receipt
  - [ ] Consumer для BookingConfirmed → Send final confirmation
  - [ ] Consumer для BookingCancelled → Send cancellation
  - [ ] Consumer для BookingExpired → Send expiration notice

- [ ] **6.5. API Endpoints**
  - [ ] `POST /api/notifications/subscribe` - подписка на уведомления
  - [ ] `GET /api/notifications/preferences` - настройки пользователя
  - [ ] `PUT /api/notifications/preferences` - обновление настроек
  - [ ] `GET /api/notifications/history` - история уведомлений

- [ ] **6.6. Notification Preferences**
  - [ ] Email notifications on/off
  - [ ] SMS notifications on/off (опционально)
  - [ ] Push notifications on/off (опционально)
  - [ ] Preferred language

- [ ] **6.7. SMS Integration (опционально)**
  - [ ] Twilio / AWS SNS integration
  - [ ] SMS templates
  - [ ] Phone number validation

- [ ] **6.8. Push Notifications (опционально)**
  - [ ] Firebase Cloud Messaging
  - [ ] Device token management
  - [ ] Push templates

- [ ] **6.9. Scheduled Notifications**
  - [ ] Check-in reminders (за 24 часа)
  - [ ] Review request (после checkout)
  - [ ] Special offers для returning customers
  - [ ] Cron jobs или scheduled tasks

- [ ] **6.10. Logging и Monitoring**
  - [ ] Логирование всех отправленных notifications
  - [ ] Статус доставки (sent, delivered, failed, bounced)
  - [ ] Retry для failed notifications
  - [ ] Metrics: send rate, delivery rate, error rate

---

### 7. Управление гостями (Guest Management)
**Статус:** ❌ **ОТСУТСТВУЕТ**

**Необходимо создать:** `services/guest-service`

#### Задачи:

- [ ] **7.1. Создать Guest Service (Go или C#)**
  - [ ] Инициализировать проект
  - [ ] Настроить database (PostgreSQL)
  - [ ] Models: Guest, Address, ContactInfo, Preferences
  - [ ] Service registration в Consul
  - [ ] Health checks

- [ ] **7.2. Guest Profile API**
  - [ ] `GET /api/guests/me` - профиль текущего пользователя
  - [ ] `PUT /api/guests/me` - обновление профиля
  - [ ] `POST /api/guests` - создание профиля гостя
  - [ ] `GET /api/guests/{id}` - получение профиля (admin)

- [ ] **7.3. Guest Data**
  - [ ] First Name, Last Name
  - [ ] Email, Phone
  - [ ] Date of Birth
  - [ ] Address (street, city, country, postal code)
  - [ ] Passport/ID information
  - [ ] Preferences (room type, floor, smoking и т.д.)

- [ ] **7.4. GDPR Compliance**
  - [ ] Data export (download my data)
  - [ ] Data deletion (right to be forgotten)
  - [ ] Data retention policy
  - [ ] Consent management

- [ ] **7.5. Integration**
  - [ ] gRPC клиент в Booking Service
  - [ ] gRPC клиент в Payment Service
  - [ ] Получение guest email для notifications

- [ ] **7.6. Guest History**
  - [ ] Список всех бронирований гостя
  - [ ] Статистика (total stays, total spent)
  - [ ] Loyalty points (опционально)

---

### 8. API Gateway
**Статус:** ⚠️ **ЧАСТИЧНО**

**Сервисы:**
- `services/api-gateway` (C# + YARP)
- `services/api-gateway-golang` (Go)

**Реализовано:**
- ✅ Reverse proxy (YARP в C#, custom в Go)
- ✅ Service discovery через Consul
- ✅ JWT authentication middleware
- ✅ Rate limiting
- ✅ Logging и metrics
- ✅ Health checks

**Файлы:**
- [Program.cs](services/api-gateway/Program.cs)
- [appsettings.json](services/api-gateway/appsettings.json)
- [main.go](services/api-gateway-golang/main.go)

#### Задачи:

- [ ] **8.1. Добавить routing для новых сервисов**
  - [ ] `/api/rooms/**` → Room Service
  - [ ] `/api/search` → Room Service (search)
  - [ ] `/api/bookings/**` → Booking Service (полный CRUD)
  - [ ] `/api/notifications/**` → Notification Service
  - [ ] `/api/guests/**` → Guest Service
  - [ ] `/api/invoices/**` → Invoice Service (когда появится)
  - [ ] `/api/reviews/**` → Review Service (когда появится)

- [ ] **8.2. Rate Limiting per User**
  - [ ] Сейчас rate limiting по клиенту
  - [ ] Добавить rate limiting по userId из JWT
  - [ ] Разные лимиты для authenticated/anonymous

- [ ] **8.3. CORS Configuration**
  - [ ] Настроить allowed origins
  - [ ] Allowed methods и headers
  - [ ] Credentials support

- [ ] **8.4. Request/Response Transformation**
  - [ ] Добавление correlation ID
  - [ ] Request tracing
  - [ ] Response standardization

- [ ] **8.5. Circuit Breaker**
  - [ ] Polly integration (C#)
  - [ ] Fallback responses при недоступности сервисов

- [ ] **8.6. API Documentation**
  - [ ] Swagger/OpenAPI endpoint
  - [ ] Агрегация документации всех сервисов

---

### 9. Frontend Applications
**Статус:** ❌ **ОТСУТСТВУЕТ** (пустые submodules)

#### Задачи:

- [ ] **9.1. Выбрать технологию**
  - [ ] React / Vue / Angular / Svelte
  - [ ] TypeScript
  - [ ] UI Framework (Material UI, Ant Design, Tailwind и т.д.)
  - [ ] State management (Redux, Zustand, Pinia и т.д.)

- [ ] **9.2. Создать Booking Frontend**
  - [ ] Проект инициализация
  - [ ] Routing setup
  - [ ] API client (axios, fetch)
  - [ ] Authentication flow (OAuth + JWT)

- [ ] **9.3. Страницы/Компоненты**
  - [ ] Home page с поиском
  - [ ] Search results page (список номеров)
  - [ ] Room details page
  - [ ] Booking form
  - [ ] Payment page
  - [ ] Booking confirmation page
  - [ ] My Bookings page (список бронирований)
  - [ ] User Profile page

- [ ] **9.4. Payment Frontend Integration**
  - [ ] Stripe Elements integration
  - [ ] Payment form
  - [ ] 3D Secure handling
  - [ ] Success/failure handling

- [ ] **9.5. Responsive Design**
  - [ ] Mobile-first approach
  - [ ] Tablet optimization
  - [ ] Desktop layout

- [ ] **9.6. Accessibility (a11y)**
  - [ ] ARIA labels
  - [ ] Keyboard navigation
  - [ ] Screen reader support
  - [ ] Color contrast

- [ ] **9.7. Internationalization (i18n)**
  - [ ] Multi-language support
  - [ ] Date/currency formatting
  - [ ] Translation files

- [ ] **9.8. Admin Frontend (опционально)**
  - [ ] Room management UI
  - [ ] Booking management UI
  - [ ] Price management UI
  - [ ] User management UI
  - [ ] Analytics dashboard

---

### 10. Testing
**Статус:** ❌ **МИНИМАЛЬНЫЙ**

#### Задачи:

- [ ] **10.1. Unit Tests**
  - [ ] Booking Service: commands, queries, handlers
  - [x] Payment Service: validator (8), builder (4), parser (11) — 24 теста (PAY-001)
  - [ ] Room Service: search, availability
  - [ ] Auth Service: token validation
  - [ ] Target coverage: минимум 70%

- [ ] **10.2. Integration Tests**
  - [ ] Booking flow end-to-end
  - [x] Payment processing: happy path, idempotency, validation, PSP failure, event publication — 5 тестов (PAY-001, ожидают прогона в CI)
  - [ ] Saga state transitions
  - [ ] Database operations
  - [ ] Message bus publishing/consuming

- [ ] **10.3. E2E Tests**
  - [ ] Happy path: search → book → pay → confirm
  - [ ] Payment failure scenario
  - [ ] Booking timeout scenario
  - [ ] Cancellation flow
  - [ ] Tools: Playwright, Cypress, Selenium

- [ ] **10.4. Load Testing**
  - [ ] Concurrent bookings
  - [ ] Search performance
  - [ ] Payment processing under load
  - [ ] Tools: k6, JMeter, Gatling
  - [ ] Target: 100 req/sec minimum

- [ ] **10.5. Contract Testing**
  - [ ] gRPC contract tests
  - [ ] Message contract tests (MassTransit)
  - [ ] API contract tests
  - [ ] Tools: Pact, Spring Cloud Contract

---

### 11. Security & Compliance
**Статус:** ⚠️ **ЧАСТИЧНО**

#### Задачи:

- [ ] **11.1. HTTPS/TLS**
  - [ ] SSL certificates для всех сервисов
  - [ ] Force HTTPS redirect
  - [ ] HSTS headers

- [ ] **11.2. Authentication & Authorization**
  - [ ] JWT validation на всех endpoints
  - [ ] Role-based access control (User, Admin)
  - [ ] Refresh token mechanism
  - [ ] Token expiration handling

- [ ] **11.3. Input Validation**
  - [ ] Request validation на всех endpoints
  - [ ] SQL injection prevention (EF Core - OK)
  - [ ] XSS prevention
  - [ ] CSRF protection

- [ ] **11.4. PCI DSS Compliance**
  - [ ] НЕ хранить credit card data
  - [ ] Использовать Stripe tokenization (уже OK)
  - [ ] Secure transmission
  - [ ] Audit logging для payment operations

- [ ] **11.5. GDPR Compliance**
  - [ ] Data encryption at rest
  - [ ] Data encryption in transit
  - [ ] Right to access (data export)
  - [ ] Right to be forgotten (data deletion)
  - [ ] Data retention policy
  - [ ] Consent management
  - [ ] Privacy policy

- [ ] **11.6. Audit Logging**
  - [ ] Логирование всех критических операций
  - [ ] Who did what when
  - [ ] Immutable audit trail
  - [ ] Log retention policy

- [ ] **11.7. Secret Management**
  - [ ] Использовать Vault / Azure Key Vault / AWS Secrets Manager
  - [ ] Ротация secrets
  - [ ] НЕ хранить secrets в коде/конфигах

- [ ] **11.8. Security Scanning**
  - [ ] SAST (Static Application Security Testing)
  - [ ] DAST (Dynamic Application Security Testing)
  - [ ] Dependency scanning (vulnerabilities)
  - [ ] Container scanning

- [ ] **11.9. Rate Limiting**
  - [ ] Per-user rate limiting (уже есть per-client)
  - [ ] DDoS protection
  - [ ] Brute force protection (login attempts)

---

### 12. Infrastructure & DevOps
**Статус:** ⚠️ **ЧАСТИЧНО**

**Реализовано:**
- ✅ Docker Compose setup
- ✅ RabbitMQ
- ✅ PostgreSQL, MongoDB
- ✅ Elasticsearch + Kibana
- ✅ Consul (service discovery) + модуль `services/consul`: docker-compose, k8s, QUICKSTART, SECRETS_MANAGEMENT.md
- ✅ Prometheus + Grafana
- ✅ Seq (structured logging)
- ✅ Инфраструктура в `infra/`: helm (booking-infra), k8s, prometheus, Makefile

**Файлы:**
- [docker-compose.yml](docker-compose.yml)
- [services/consul/README.md](services/consul/README.md)
- [services/consul/SECRETS_MANAGEMENT.md](services/consul/SECRETS_MANAGEMENT.md)
- Kubernetes configs (частично в `kubernetes/`, `infra/`)

#### Задачи:

- [ ] **12.1. Kubernetes Deployment**
  - [ ] Создать K8s manifests для всех сервисов
  - [ ] ConfigMaps для конфигурации
  - [ ] Secrets для sensitive data
  - [ ] Services и Ingress
  - [ ] Horizontal Pod Autoscaling

- [ ] **12.2. CI/CD Pipeline**
  - [ ] GitHub Actions / GitLab CI / Jenkins
  - [ ] Build stage (compile, test)
  - [ ] Security scan stage
  - [ ] Docker image build и push
  - [ ] Deploy to staging
  - [ ] Deploy to production (manual approval)

- [ ] **12.3. Monitoring & Alerting**
  - [ ] Prometheus metrics для всех сервисов
  - [ ] Grafana dashboards
  - [ ] Alerting rules (high error rate, high latency и т.д.)
  - [ ] PagerDuty / OpsGenie integration

- [ ] **12.4. Logging**
  - [ ] Structured logging во всех сервисах
  - [ ] Centralized logging (ELK уже есть, настроить)
  - [ ] Log correlation IDs
  - [ ] Log retention policy

- [ ] **12.5. Distributed Tracing**
  - [ ] Jaeger / Zipkin setup
  - [ ] Instrumentation во всех сервисах
  - [ ] Trace visualization

- [ ] **12.6. Backup & Disaster Recovery**
  - [ ] Database backups (automated)
  - [ ] Backup retention policy
  - [ ] Disaster recovery plan
  - [ ] RTO/RPO targets

- [ ] **12.7. Environment Management**
  - [ ] Development environment
  - [ ] Staging environment (копия production)
  - [ ] Production environment
  - [ ] Environment parity

---

### 13. Дополнительные сервисы (Nice to Have)

#### 13.1. Invoice Service
**Статус:** ❌ **ОТСУТСТВУЕТ**

- [ ] Создать Invoice Service
- [ ] Generate PDF invoices
- [ ] Email invoices автоматически
- [ ] Invoice storage
- [ ] Tax calculation
- [ ] Integration с accounting systems

#### 13.2. Review Service
**Статус:** ❌ **ОТСУТСТВУЕТ**

- [ ] Создать Review Service
- [ ] Guest reviews API
- [ ] Rating system (1-5 stars)
- [ ] Review moderation
- [ ] Response from management
- [ ] Average rating calculation

#### 13.3. Loyalty Program
**Статус:** ❌ **ОТСУТСТВУЕТ**

- [ ] Points system
- [ ] Reward tiers
- [ ] Points redemption
- [ ] Special offers для loyalty members

#### 13.4. Analytics Service
**Статус:** ❌ **ОТСУТСТВУЕТ**

- [ ] Booking analytics
- [ ] Revenue reports
- [ ] Occupancy reports
- [ ] Customer insights
- [ ] Forecasting

#### 13.5. Chat/Support Service
**Статус:** ❌ **ОТСУТСТВУЕТ**

- [ ] Live chat
- [ ] Chatbot integration
- [ ] Support ticket system
- [ ] FAQ management

---

## Приоритизация задач

### Фаза 1: MVP (Minimum Viable Product)
**Цель:** Базовый working flow от поиска до подтверждения

1. ✅ Authentication Service (ГОТОВО)
2. ✅ Room Service (ГОТОВО: CRUD, search, availability, gRPC, Consul)
3. ✅ Pricing Service (ГОТОВО: расчёт цен, стратегии, gRPC)
4. 🔧 Завершить Booking Service (confirm/cancel готовы; осталось: list, validation, интеграция с Room)
5. ⚠️ Завершить Payment Service — **PAY-001 done**, middleware refactor сделан; осталось: PAY-002 (webhook HMAC), PAY-003 (status enum), дальнейшее выравнивание application-layer orchestration
6. 🔧 Notification Service (email only; репозиторий — заглушка)
7. 🔧 Guest Service (базовый профиль)
8. 🔧 Frontend (директории есть, приложений нет — начать Phase 0)
9. ⚠️ Integration testing — Payment Service: 24 unit + 5 integration тестов (PAY-001)

**ETA:** 3-5 недель (с учётом готовых Room/Pricing)

### Фаза 2: Production Ready
**Цель:** Security, compliance, stability

1. 🔧 PMS Integration (реальная или собственная)
2. 🔧 Security hardening (HTTPS, validation, audit logs)
3. 🔧 GDPR compliance
4. 🔧 Comprehensive testing (unit, integration, E2E)
5. 🔧 Monitoring и alerting
6. 🔧 CI/CD pipeline
7. 🔧 Kubernetes deployment
8. 🔧 Load testing и optimization

**ETA:** 3-4 недели

### Фаза 3: Enhancements
**Цель:** Дополнительные features

1. 🔧 Invoice Service
2. 🔧 Review Service
3. 🔧 SMS notifications
4. 🔧 Multiple payment methods
5. 🔧 Loyalty program
6. 🔧 Analytics
7. 🔧 Admin frontend
8. 🔧 Mobile apps

**ETA:** Ongoing

---

## Метрики успеха

### Technical Metrics
- [ ] Test coverage > 70%
- [ ] API response time < 200ms (p95)
- [ ] Payment processing time < 3s
- [ ] Search results < 1s
- [ ] Uptime > 99.9%
- [ ] Zero data breaches
- [ ] Zero PCI DSS violations

### Business Metrics
- [ ] Успешный booking completion rate > 80%
- [ ] Payment success rate > 95%
- [ ] Customer satisfaction > 4.5/5
- [ ] Average booking time < 5 minutes

---

## Команда и роли

### Backend Development
- Go services: auth, room, notification (опционально)
- C# services: booking, payment
- API Gateway: оба варианта

### Frontend Development
- React/Vue/Angular
- Responsive design
- UX optimization

### DevOps
- Kubernetes
- CI/CD
- Monitoring

### QA
- Test automation
- Security testing
- Load testing

---

## Следующие шаги

1. ✅ Создан roadmap документ
2. ✅ Room Service и Pricing Service реализованы
3. ✅ PAY-001: Payment Intent — убраны hardcoded значения, добавлена конфигурация, idempotency, валидация, тесты
4. 🔧 Добавить в API Gateway маршруты `/api/rooms/**`, `/api/bookings/**`, `/api/pricing/**`
5. 🔧 Реализовать confirm/cancel в Booking Service (снять заглушки)
6. 🔧 Завершить Payment Service: PAY-002 (webhook HMAC security), quote/refund TODOs
7. 🔧 Notification Service и Guest Service (создать с нуля)
8. 🔧 Frontend: инициализировать монорепо (Phase 0 FRONTEND_ROADMAP)
9. 🔧 Integration testing — прогнать Payment integration tests в полном окружении

---

## Примечания

- Документ актуален на: **2026-05-03** (обновлён после booking confirm/cancel и middleware refactor)
- Обновлять после завершения каждой задачи
- Приоритеты могут меняться
- Время оценки приблизительное

---

**Готовы начать? Выберите первую задачу из Фазы 1!** 🚀
