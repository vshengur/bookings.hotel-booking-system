# Hotel Booking System - Frontend Development Roadmap

## Дата создания: 2026-01-19

---

## Оглавление

0. [Текущий статус реализации](#0-текущий-статус-реализации)
1. [Технологический стек и архитектура](#1-технологический-стек-и-архитектура)
2. [Минимальный набор экранов (MVP)](#2-минимальный-набор-экранов-mvp)
3. [Поэтапный план разработки](#3-поэтапный-план-разработки)
4. [Интеграция с Backend API](#4-интеграция-с-backend-api)
5. [Auth Flow архитектура](#5-auth-flow-архитектура)
6. [WebSocket интеграция](#6-websocket-интеграция)
7. [Design System сравнение](#7-design-system-сравнение)
8. [Критерии готовности](#8-критерии-готовности)

---

## 0. Текущий статус реализации

**Дата актуализации: 2026-02-22** (обновлён после PAY-001)

### Легенда
- ✅ **ГОТОВО** — компонент реализован
- ⚠️ **ЧАСТИЧНО** — есть заготовки, не завершено
- ❌ **НЕ НАЧАТО** — не реализовано

### Инфраструктура
| Элемент | Статус | Примечание |
|--------|--------|------------|
| Директория `frontend/` | ✅ | Есть в корне проекта |
| Монорепо (pnpm workspaces, Turborepo) | ❌ | Не инициализирован |
| `apps/guest-frontend` (Next.js) | ❌ | Нет (есть только `booking-frontend` как пустой репозиторий) |
| `apps/payment-frontend` | ❌ | Папка есть, только README/LICENSE |
| `apps/notification-widget` | ❌ | Нет (есть `notification-frontend` — пустой репозиторий) |
| `packages/*` (api-client, auth-sdk, types и т.д.) | ❌ | Нет |

### Текущее состояние репозиториев frontend
- **frontend/booking-frontend** — placeholder (README «bookings.booking-frontend», LICENSE). Кода приложения нет.
- **frontend/payment-frontend** — placeholder (README, LICENSE). Кода нет.
- **frontend/notification-frontend** — placeholder. Кода нет.

### Backend для фронтенда (готовность API)
| API / сервис | Статус | Комментарий |
|--------------|--------|-------------|
| Auth (Google OAuth, JWT) | ✅ | auth-service готов |
| Room Search, CRUD, Availability | ✅ | room-service готов |
| Pricing / Quote | ✅ | pricing-service готов |
| Booking (create, get, confirm, cancel) | ⚠️ | bookings-service: create/get есть, confirm/cancel — заглушки |
| Payment (intent, webhook) | ⚠️ | payment-service: intent **обновлён** (PAY-001) — принимает `{ bookingId, amount, currency }`, idempotency, валидация. Осталось: webhook HMAC (PAY-002) |
| Guest Profile | ❌ | guest-service не создан |
| Notifications | ❌ | notification-service — заглушка |
| API Gateway routing для /api/rooms, /api/bookings | ❌ | Маршруты не добавлены |

### Рекомендуемые следующие шаги
1. Инициализировать монорепо в `frontend/`: pnpm-workspace, turbo.json, структура `apps/` и `packages/`.
2. Создать Guest Frontend (Next.js) в `apps/guest-frontend` по плану Phase 0–2.
3. Добавить в API Gateway маршруты к room-service, bookings-service, pricing-service.
4. Реализовать confirm/cancel в bookings-service. Payment intent обновлён (PAY-001), осталось: PAY-002 (webhook HMAC).

---

## 1. Технологический стек и архитектура

### 1.1 Выбор технологий по микрофронтендам

| Микрофронтенд | Технология | Обоснование |
|---------------|-----------|-------------|
| **Guest Frontend** (booking-frontend) | **Next.js 15 + React 19** | - SSR для SEO (поиск номеров)<br>- App Router для лучшего DX<br>- Серверные компоненты для performance<br>- ISR для кеширования поисковых результатов |
| **Payment Frontend** (payment-frontend) | **React 19 + Vite** | - SPA без SEO требований<br>- Быстрая сборка (Vite)<br>- Stripe Elements integration<br>- Не требует SSR |
| **Admin Frontend** (admin-frontend) | **Angular 19** | - Enterprise-grade (CRUD operations)<br>- TypeScript first<br>- Встроенная DI<br>- RxJS для сложных форм<br>- Material Design готов |
| **Notification Center** (notification-frontend) | **React 19 + Vite** | - Легковесный виджет<br>- WebSocket real-time<br>- Может встраиваться в другие фронтенды |

### 1.2 Общая архитектура

```
┌─────────────────────────────────────────────────────────┐
│                  Browser / Client                        │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │ Guest Frontend│  │Payment Widget│  │Admin Frontend│  │
│  │  (Next.js)   │  │   (React)    │  │  (Angular)   │  │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘  │
│         │                  │                  │          │
│         └──────────────────┴──────────────────┘          │
│                            ▼                             │
│                   ┌────────────────┐                     │
│                   │  Shared Layer  │                     │
│                   │  - Auth SDK    │                     │
│                   │  - API Client  │                     │
│                   │  - WS Manager  │                     │
│                   └────────┬───────┘                     │
│                            ▼                             │
└────────────────────────────┼─────────────────────────────┘
                             │
                             ▼
                    ┌────────────────┐
                    │  API Gateway   │
                    │  (YARP / Go)   │
                    └────────────────┘
```

### 1.3 Общие библиотеки (Shared Packages)

```typescript
// packages/api-client/
@bookings/api-client      // Axios client с interceptors

// packages/auth-sdk/
@bookings/auth-sdk        // Auth manager, token refresh, OAuth flow

// packages/websocket/
@bookings/websocket       // WebSocket connection manager

// packages/ui-primitives/
@bookings/ui-primitives   // Общие компоненты (Button, Input и т.д.)

// packages/types/
@bookings/types           // TypeScript definitions (sync с backend DTOs)
```

**Инструмент для монорепо:**
- **pnpm workspaces** - быстрый, экономит место
- **Turborepo** - для оптимизации сборки

---

## 2. Минимальный набор экранов (MVP)

### 2.1 Guest Frontend - User Journey

```
┌─────────────────────┐
│  1. Home / Search   │  ← Старт
└─────────┬───────────┘
          ▼
┌─────────────────────┐
│  2. Search Results  │  (список номеров)
└─────────┬───────────┘
          ▼
┌─────────────────────┐
│  3. Room Details    │  (детали номера)
└─────────┬───────────┘
          ▼
┌─────────────────────┐
│  4. Booking Form    │  (гостевые данные, промокод)
└─────────┬───────────┘
          ▼
┌─────────────────────┐
│  5. Payment Page    │  (Stripe Elements)
└─────────┬───────────┘
          ▼
┌─────────────────────┐
│  6. Confirmation    │  (номер брони, инструкции)
└─────────────────────┘

┌─────────────────────┐
│  7. My Bookings     │  (список бронирований пользователя)
└─────────────────────┘

┌─────────────────────┐
│  8. User Profile    │  (редактирование профиля)
└─────────────────────┘
```

### 2.2 Детализация экранов

#### Экран 1: Home / Search
**Путь:** `/`

**Компоненты:**
- Hero section с поисковой формой
- Форма поиска:
  - Check-in date (date picker)
  - Check-out date (date picker)
  - Adults count (number input)
  - Children count (number input)
  - CTA button "Search Rooms"
- Featured rooms (карусель топовых номеров)
- Trust indicators (reviews, ratings)

**API:**
- `GET /api/rooms/featured` (или mock)

---

#### Экран 2: Search Results
**Путь:** `/search?checkIn=2026-02-15&checkOut=2026-02-20&adults=2&children=0`

**Компоненты:**
- Search summary bar (выбранные даты, гости)
- Filters sidebar:
  - Price range (slider)
  - Amenities (checkboxes: WiFi, TV, балкон и т.д.)
  - Capacity (dropdown)
- Room cards grid:
  - Image carousel (3-5 фото)
  - Room type, capacity
  - Price per night + total price
  - Amenities icons
  - "View Details" button
- Pagination или infinite scroll
- "No results" state

**API:**
- `GET /api/rooms/search?checkIn=...&checkOut=...&adults=...&children=...&minPrice=...&maxPrice=...&amenities=...`

**Mock data strategy:**
- Если Room Service не готов → использовать mock JSON с 10-15 номерами
- Файл: `src/mocks/rooms.json`

---

#### Экран 3: Room Details
**Путь:** `/rooms/:roomId?checkIn=...&checkOut=...`

**Компоненты:**
- Image gallery (полноэкранный просмотр)
- Room title, description
- Amenities list (иконки + названия)
- Availability calendar (показать занятые даты)
- Pricing breakdown:
  - Price per night × nights count
  - Taxes & fees
  - **Total price**
- Guest reviews section (если Review Service готов)
- Sticky booking panel:
  - Dates summary
  - Guests count
  - Total price
  - "Book Now" button

**API:**
- `GET /api/rooms/:roomId`
- `GET /api/rooms/:roomId/availability?from=...&to=...`
- `GET /api/pricing/quote?roomId=...&checkIn=...&checkOut=...` (или mock)

---

#### Экран 4: Booking Form
**Путь:** `/booking/new?roomId=...&checkIn=...&checkOut=...`

**Компоненты:**
- Booking summary (даты, номер, цена)
- Guest information form:
  - First Name, Last Name (required)
  - Email (required, validation)
  - Phone (required, international format)
  - Special requests (textarea, optional)
- Promo code input:
  - Input field + "Apply" button
  - Validation message (success/error)
  - Updated total price
- Terms & conditions checkbox (required)
- "Proceed to Payment" button

**API:**
- `POST /api/bookings` → создает бронирование со статусом `Pending`
  ```json
  {
    "roomId": "uuid",
    "checkInDate": "2026-02-15",
    "checkOutDate": "2026-02-20",
    "guestInfo": {
      "firstName": "John",
      "lastName": "Doe",
      "email": "john@example.com",
      "phone": "+1234567890"
    },
    "promoCode": "SUMMER20",
    "specialRequests": "Late check-in"
  }
  ```
- Response: `{ "bookingId": "uuid", "status": "Pending", "expiresAt": "2026-01-19T12:30:00Z" }`

**Mock strategy:**
- Если Booking Service не готов → localStorage сохранение + генерация uuid
- Редирект на Payment с `bookingId` в query params

---

#### Экран 5: Payment Page
**Путь:** `/booking/:bookingId/payment`

**Компоненты:**
- Booking summary (read-only)
- Payment countdown timer (15 минут до истечения)
- Stripe Elements Card Input (PCI compliant)
- Billing address form (optional для Stripe)
- Security badges (SSL, Stripe, PCI DSS)
- "Pay Now" button (с лоадером)
- Cancel link (возврат к Booking Form)

**API:**
- `POST /api/payment/intent` → создает Payment Intent
  ```json
  {
    "bookingId": "uuid",
    "amount": 25000, // cents
    "currency": "EUR"
  }
  ```
- Response: `{ "clientSecret": "pi_xxx_secret_yyy" }`

- **Stripe Elements** обработка:
  ```typescript
  const { error, paymentIntent } = await stripe.confirmCardPayment(clientSecret, {
    payment_method: {
      card: cardElement,
      billing_details: { name, email }
    }
  });
  ```

- После успешной оплаты → webhook от Stripe обновляет статус в backend
- Frontend слушает через WebSocket: `BookingStatusChanged` event

**Mock strategy:**
- Использовать Stripe Test Mode (ключи `pk_test_...`)
- Test card: `4242 4242 4242 4242`, любая дата/CVC

---

#### Экран 6: Confirmation Page
**Путь:** `/booking/:bookingId/confirmation`

**Компоненты:**
- Success checkmark animation
- Booking confirmation number (крупный шрифт)
- Booking details:
  - Room info
  - Dates
  - Guest info
  - Total paid
- Next steps:
  - "Confirmation email sent to ..."
  - Check-in instructions
  - Cancellation policy
- Action buttons:
  - "View Booking" → My Bookings
  - "Download Receipt" (PDF)
  - "Add to Calendar" (ICS file)

**API:**
- `GET /api/bookings/:bookingId`

**Mock strategy:**
- Показать данные из localStorage если нет backend

---

#### Экран 7: My Bookings
**Путь:** `/my-bookings`

**Требует:** Авторизацию (JWT)

**Компоненты:**
- Tabs:
  - Upcoming (статусы: Confirmed, Reserved)
  - Past (статусы: CheckedOut)
  - Cancelled (статус: Cancelled)
- Booking cards:
  - Confirmation number
  - Room image
  - Dates
  - Status badge
  - Total price
  - Actions:
    - "View Details"
    - "Cancel Booking" (если cancellation policy позволяет)
    - "Download Invoice"
- Empty state (для каждого таба)

**API:**
- `GET /api/bookings?userId=...&status=...`

**Mock strategy:**
- Локальный массив бронирований в localStorage

---

#### Экран 8: User Profile
**Путь:** `/profile`

**Требует:** Авторизацию (JWT)

**Компоненты:**
- Profile section:
  - Avatar upload
  - First Name, Last Name
  - Email (read-only, от Google OAuth)
  - Phone
- Preferences:
  - Language (dropdown)
  - Currency (dropdown)
  - Notification preferences (checkboxes)
- Saved payment methods (опционально для MVP)
- "Save Changes" button
- "Logout" button

**API:**
- `GET /api/guests/me`
- `PUT /api/guests/me`

**Mock strategy:**
- localStorage для профиля

---

## 3. Поэтапный план разработки

### PHASE 0: Подготовка инфраструктуры (Week 1)

**Цель:** Настроить окружение, монорепо, CI/CD базу

#### Задачи:

**0.1 Инициализация монорепо**
- [x] Создать `frontend/` директорию в корне проекта (есть)
- [ ] Инициализировать pnpm workspace
  ```json
  // pnpm-workspace.yaml
  packages:
    - 'apps/*'
    - 'packages/*'
  ```
- [ ] Установить Turborepo
  ```bash
  pnpm add -Dw turbo
  ```
- [ ] Создать `turbo.json` конфигурацию
  ```json
  {
    "$schema": "https://turbo.build/schema.json",
    "pipeline": {
      "build": {
        "dependsOn": ["^build"],
        "outputs": [".next/**", "dist/**"]
      },
      "dev": {
        "cache": false
      },
      "lint": {},
      "test": {}
    }
  }
  ```

**0.2 Создать структуру проектов**
```
frontend/
├── apps/
│   ├── guest-frontend/          # Next.js 15
│   ├── payment-frontend/        # React 19 + Vite
│   ├── admin-frontend/          # Angular 19
│   └── notification-widget/     # React 19 + Vite
├── packages/
│   ├── api-client/              # Shared API client
│   ├── auth-sdk/                # Auth manager
│   ├── websocket/               # WebSocket manager
│   ├── ui-primitives/           # Common components
│   ├── types/                   # TypeScript types
│   └── config/                  # Shared configs (eslint, tsconfig и т.д.)
├── pnpm-workspace.yaml
├── turbo.json
├── .eslintrc.js
├── .prettierrc
└── README.md
```

**0.3 Настроить TypeScript конфигурацию**
- [ ] Создать base `tsconfig.json` в `packages/config/`
- [ ] Extend в каждом проекте
- [ ] Strict mode enabled
- [ ] Path aliases настроены (`@/`, `~/`)

**0.4 Настроить линтеры и форматеры**
- [ ] ESLint с TypeScript support
- [ ] Prettier
- [ ] Husky + lint-staged для pre-commit hooks
  ```json
  {
    "*.{ts,tsx}": ["eslint --fix", "prettier --write"],
    "*.{json,md}": ["prettier --write"]
  }
  ```

**0.5 Инициализировать проекты**

**Guest Frontend (Next.js):**
```bash
cd frontend/apps
pnpm create next-app guest-frontend --typescript --tailwind --app --no-src-dir
```

**Payment Frontend (React + Vite):**
```bash
pnpm create vite payment-frontend --template react-ts
```

**Admin Frontend (Angular):**
```bash
npx @angular/cli new admin-frontend --routing --style=scss --strict
```

**0.6 Docker setup для разработки**
- [ ] Создать `docker-compose.dev.yml` для frontend сервисов
  ```yaml
  version: '3.8'
  services:
    guest-frontend:
      build: ./apps/guest-frontend
      ports:
        - "3000:3000"
      volumes:
        - ./apps/guest-frontend:/app
      environment:
        - API_GATEWAY_URL=http://api-gateway:8080
        - WS_URL=ws://api-gateway:8080/ws

    payment-frontend:
      build: ./apps/payment-frontend
      ports:
        - "3001:3001"

    admin-frontend:
      build: ./apps/admin-frontend
      ports:
        - "4200:4200"
  ```

**0.7 Environment variables setup**
- [ ] Создать `.env.example` для каждого проекта
  ```env
  # Guest Frontend (.env.local)
  NEXT_PUBLIC_API_URL=http://localhost:8080/api
  NEXT_PUBLIC_WS_URL=ws://localhost:8080/ws
  NEXT_PUBLIC_STRIPE_PK=pk_test_...
  NEXT_PUBLIC_GOOGLE_OAUTH_CLIENT_ID=...
  ```

---

### PHASE 1: Shared Packages (Week 1-2)

**Цель:** Создать переиспользуемые библиотеки для всех фронтендов

#### Задачи:

**1.1 @bookings/types**
- [ ] Создать TypeScript definitions
  ```typescript
  // packages/types/src/booking.ts
  export interface Booking {
    id: string;
    roomId: string;
    checkInDate: string;
    checkOutDate: string;
    guestInfo: GuestInfo;
    status: BookingStatus;
    totalPrice: number;
    currency: string;
    createdAt: string;
    expiresAt?: string;
  }

  export enum BookingStatus {
    Pending = 'Pending',
    AwaitingPayment = 'AwaitingPayment',
    Reserved = 'Reserved',
    Confirmed = 'Confirmed',
    Cancelled = 'Cancelled',
    Expired = 'Expired',
    Failed = 'Failed'
  }

  export interface GuestInfo {
    firstName: string;
    lastName: string;
    email: string;
    phone: string;
  }

  // ... остальные типы (Room, Payment, User и т.д.)
  ```
- [ ] Настроить сборку: `tsc -b`
- [ ] Экспортировать в `index.ts`

**1.2 @bookings/api-client**
- [ ] Установить зависимости: `axios`, `ky` (альтернатива)
- [ ] Создать Axios instance с interceptors
  ```typescript
  // packages/api-client/src/client.ts
  import axios from 'axios';
  import type { AxiosInstance } from 'axios';

  export class ApiClient {
    private client: AxiosInstance;

    constructor(baseURL: string, private authManager?: AuthManager) {
      this.client = axios.create({
        baseURL,
        timeout: 10000,
        headers: {
          'Content-Type': 'application/json'
        }
      });

      this.setupInterceptors();
    }

    private setupInterceptors() {
      // Request interceptor - добавить JWT
      this.client.interceptors.request.use(
        async (config) => {
          const token = await this.authManager?.getAccessToken();
          if (token) {
            config.headers.Authorization = `Bearer ${token}`;
          }
          return config;
        },
        (error) => Promise.reject(error)
      );

      // Response interceptor - обработка ошибок
      this.client.interceptors.response.use(
        (response) => response,
        async (error) => {
          if (error.response?.status === 401) {
            // Token expired - попробовать refresh
            const refreshed = await this.authManager?.refreshToken();
            if (refreshed) {
              // Retry original request
              return this.client.request(error.config);
            }
          }
          return Promise.reject(error);
        }
      );
    }

    // API methods
    async get<T>(url: string, config?: any): Promise<T> {
      const response = await this.client.get<T>(url, config);
      return response.data;
    }

    async post<T>(url: string, data?: any, config?: any): Promise<T> {
      const response = await this.client.post<T>(url, data, config);
      return response.data;
    }

    // ... put, delete, patch
  }
  ```

- [ ] Создать сервисы для каждого домена:
  ```typescript
  // packages/api-client/src/services/BookingService.ts
  import type { Booking, CreateBookingDto } from '@bookings/types';
  import { ApiClient } from '../client';

  export class BookingService {
    constructor(private client: ApiClient) {}

    async createBooking(dto: CreateBookingDto): Promise<Booking> {
      return this.client.post<Booking>('/bookings', dto);
    }

    async getBooking(id: string): Promise<Booking> {
      return this.client.get<Booking>(`/bookings/${id}`);
    }

    async getUserBookings(userId: string): Promise<Booking[]> {
      return this.client.get<Booking[]>(`/bookings?userId=${userId}`);
    }

    async cancelBooking(id: string): Promise<void> {
      return this.client.post<void>(`/bookings/${id}/cancel`);
    }
  }

  // Аналогично: RoomService, PaymentService, AuthService, GuestService
  ```

**1.3 @bookings/auth-sdk**
- [ ] Реализовать AuthManager (из рекомендаций выше)
  ```typescript
  // packages/auth-sdk/src/AuthManager.ts
  import Cookies from 'js-cookie';

  export class AuthManager {
    private accessToken: string | null = null;
    private refreshTimer: NodeJS.Timeout | null = null;

    constructor(
      private authServiceUrl: string,
      private onAuthStateChange?: (authenticated: boolean) => void
    ) {
      this.loadTokenFromStorage();
    }

    private loadTokenFromStorage() {
      // На клиенте проверяем httpOnly cookie через API call
      this.validateSession();
    }

    async login(credentials?: { email: string; password: string }): Promise<void> {
      // Если credentials нет → Google OAuth redirect
      if (!credentials) {
        window.location.href = `${this.authServiceUrl}/login`;
        return;
      }

      // Email/password login (если реализован)
      const response = await fetch(`${this.authServiceUrl}/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(credentials),
        credentials: 'include' // для httpOnly cookie
      });

      const data = await response.json();
      this.setAccessToken(data.accessToken, data.expiresIn);
    }

    async handleOAuthCallback(code: string): Promise<void> {
      // После redirect от Google
      const response = await fetch(`${this.authServiceUrl}/callback?code=${code}`, {
        credentials: 'include'
      });

      const data = await response.json();
      this.setAccessToken(data.accessToken, data.expiresIn);
    }

    private setAccessToken(token: string, expiresIn: number) {
      this.accessToken = token;
      this.scheduleTokenRefresh(expiresIn - 60); // 1 min before expiry
      this.onAuthStateChange?.(true);
    }

    private scheduleTokenRefresh(seconds: number) {
      if (this.refreshTimer) clearTimeout(this.refreshTimer);

      this.refreshTimer = setTimeout(() => {
        this.refreshToken();
      }, seconds * 1000);
    }

    async refreshToken(): Promise<boolean> {
      try {
        const response = await fetch(`${this.authServiceUrl}/refresh`, {
          method: 'POST',
          credentials: 'include' // refresh token в httpOnly cookie
        });

        if (!response.ok) {
          this.logout();
          return false;
        }

        const data = await response.json();
        this.setAccessToken(data.accessToken, data.expiresIn);
        return true;
      } catch (error) {
        this.logout();
        return false;
      }
    }

    getAccessToken(): string | null {
      return this.accessToken;
    }

    async validateSession(): Promise<boolean> {
      try {
        const response = await fetch(`${this.authServiceUrl}/validate-token`, {
          headers: this.accessToken ? {
            Authorization: `Bearer ${this.accessToken}`
          } : {},
          credentials: 'include'
        });

        if (response.ok) {
          const data = await response.json();
          if (data.valid) {
            this.setAccessToken(this.accessToken || data.accessToken, data.expiresIn);
            return true;
          }
        }

        return false;
      } catch {
        return false;
      }
    }

    logout() {
      this.accessToken = null;
      if (this.refreshTimer) clearTimeout(this.refreshTimer);

      // Очистить refresh token cookie на сервере
      fetch(`${this.authServiceUrl}/logout`, {
        method: 'POST',
        credentials: 'include'
      });

      this.onAuthStateChange?.(false);
    }

    isAuthenticated(): boolean {
      return this.accessToken !== null;
    }
  }
  ```

**1.4 @bookings/websocket**
- [ ] Реализовать WebSocket manager для real-time updates
  ```typescript
  // packages/websocket/src/WebSocketManager.ts
  import { EventEmitter } from 'events';

  export interface WSMessage {
    type: string;
    payload: any;
  }

  export class WebSocketManager extends EventEmitter {
    private ws: WebSocket | null = null;
    private reconnectTimer: NodeJS.Timeout | null = null;
    private reconnectAttempts = 0;
    private maxReconnectAttempts = 5;

    constructor(
      private url: string,
      private getAccessToken: () => string | null
    ) {
      super();
    }

    connect() {
      const token = this.getAccessToken();
      const wsUrl = token ? `${this.url}?token=${token}` : this.url;

      this.ws = new WebSocket(wsUrl);

      this.ws.onopen = () => {
        console.log('[WS] Connected');
        this.reconnectAttempts = 0;
        this.emit('connected');
      };

      this.ws.onmessage = (event) => {
        try {
          const message: WSMessage = JSON.parse(event.data);
          this.emit(message.type, message.payload);
          this.emit('message', message);
        } catch (error) {
          console.error('[WS] Failed to parse message', error);
        }
      };

      this.ws.onerror = (error) => {
        console.error('[WS] Error', error);
        this.emit('error', error);
      };

      this.ws.onclose = () => {
        console.log('[WS] Disconnected');
        this.emit('disconnected');
        this.scheduleReconnect();
      };
    }

    private scheduleReconnect() {
      if (this.reconnectAttempts >= this.maxReconnectAttempts) {
        console.error('[WS] Max reconnect attempts reached');
        return;
      }

      const delay = Math.min(1000 * 2 ** this.reconnectAttempts, 30000);
      this.reconnectAttempts++;

      this.reconnectTimer = setTimeout(() => {
        console.log(`[WS] Reconnecting (attempt ${this.reconnectAttempts})...`);
        this.connect();
      }, delay);
    }

    send(type: string, payload: any) {
      if (this.ws?.readyState === WebSocket.OPEN) {
        this.ws.send(JSON.stringify({ type, payload }));
      } else {
        console.warn('[WS] Cannot send - not connected');
      }
    }

    disconnect() {
      if (this.reconnectTimer) clearTimeout(this.reconnectTimer);
      this.ws?.close();
      this.ws = null;
    }

    // Subscribe to specific event types
    on(event: string, listener: (...args: any[]) => void): this {
      return super.on(event, listener);
    }
  }

  // Usage in components:
  // wsManager.on('BookingStatusChanged', (payload) => {
  //   console.log('Booking status updated', payload);
  // });
  ```

**1.5 @bookings/ui-primitives**
- [ ] Выбрать один из трех Design Systems для экспериментов:

  **Вариант A: Shadcn/ui + Tailwind** (Рекомендуется)
  - Установка: copy-paste компонентов (не npm пакет)
  - Customizable
  - Работает с Next.js, React, Vue
  - [shadcn/ui](https://ui.shadcn.com/)

  **Вариант B: Chakra UI**
  - Простой в использовании
  - Accessibility из коробки
  - Хорошая документация
  - [Chakra UI](https://chakra-ui.com/)

  **Вариант C: Mantine**
  - TypeScript first
  - 100+ компонентов
  - Hooks library
  - [Mantine](https://mantine.dev/)

- [ ] Создать базовые компоненты:
  - Button
  - Input
  - Select
  - DatePicker
  - Modal
  - Card
  - Badge
  - Spinner
  - Toast/Notification

- [ ] Настроить theme/tokens:
  ```typescript
  // packages/ui-primitives/src/theme/tokens.ts
  export const colors = {
    primary: {
      50: '#e3f2fd',
      100: '#bbdefb',
      // ... до 900
    },
    // ... остальные цвета
  };

  export const spacing = {
    xs: '0.25rem',
    sm: '0.5rem',
    md: '1rem',
    lg: '1.5rem',
    xl: '2rem',
  };
  ```

**Mock Data Package**
- [ ] Создать `packages/mock-data/`
  ```typescript
  // packages/mock-data/src/rooms.ts
  import type { Room } from '@bookings/types';

  export const mockRooms: Room[] = [
    {
      id: 'room-1',
      name: 'Deluxe King Room',
      description: 'Spacious room with king-size bed...',
      capacity: 2,
      pricePerNight: 150,
      currency: 'EUR',
      images: [
        'https://images.unsplash.com/photo-1...',
      ],
      amenities: ['wifi', 'tv', 'air-conditioning', 'minibar'],
      available: true
    },
    // ... еще 10-15 номеров
  ];

  // packages/mock-data/src/index.ts
  export * from './rooms';
  export * from './bookings';
  export * from './users';
  ```

- [ ] Создать mock API wrapper (для переключения между mock/real API):
  ```typescript
  // packages/api-client/src/MockableApiClient.ts
  import { ApiClient } from './client';
  import { mockRooms } from '@bookings/mock-data';

  export class MockableApiClient extends ApiClient {
    constructor(baseURL: string, private useMock: boolean = false) {
      super(baseURL);
    }

    async get<T>(url: string, config?: any): Promise<T> {
      if (this.useMock) {
        return this.getMockData(url) as T;
      }
      return super.get<T>(url, config);
    }

    private getMockData(url: string): any {
      // Simple routing
      if (url.startsWith('/rooms')) {
        return mockRooms;
      }
      // ... остальные endpoints
      throw new Error(`No mock data for ${url}`);
    }
  }
  ```

**API Готовность Tracker**
- [ ] Создать feature flag конфигурацию:
  ```typescript
  // packages/config/src/features.ts
  export const apiFeatures = {
    roomService: {
      enabled: false, // Switch to true when ready
      mockFallback: true
    },
    bookingService: {
      enabled: true,
      mockFallback: false
    },
    paymentService: {
      enabled: true,
      mockFallback: false
    },
    guestService: {
      enabled: false,
      mockFallback: true
    },
    notificationService: {
      enabled: false,
      mockFallback: true
    }
  };
  ```

---

### PHASE 2: Guest Frontend - Search & Browse (Week 2-3)

**Цель:** Реализовать экраны поиска и просмотра номеров

**Технология:** Next.js 15 (App Router)

#### Задачи:

**2.1 Настроить Next.js проект**
- [ ] Установить зависимости:
  ```bash
  pnpm add @bookings/api-client @bookings/auth-sdk @bookings/types @bookings/ui-primitives
  pnpm add react-hook-form zod @hookform/resolvers
  pnpm add date-fns react-day-picker
  pnpm add @stripe/stripe-js @stripe/react-stripe-js
  pnpm add zustand # state management
  ```

- [ ] Настроить `next.config.js`:
  ```javascript
  /** @type {import('next').NextConfig} */
  const nextConfig = {
    reactStrictMode: true,
    images: {
      domains: ['images.unsplash.com', 'your-cdn.com'],
    },
    env: {
      API_URL: process.env.NEXT_PUBLIC_API_URL,
      WS_URL: process.env.NEXT_PUBLIC_WS_URL,
      STRIPE_PK: process.env.NEXT_PUBLIC_STRIPE_PK,
    },
  };

  module.exports = nextConfig;
  ```

**2.2 Создать layout и navigation**
- [ ] `app/layout.tsx` - root layout:
  ```tsx
  import { Inter } from 'next/font/google';
  import { Providers } from './providers';
  import { Header } from '@/components/Header';
  import { Footer } from '@/components/Footer';

  const inter = Inter({ subsets: ['latin'] });

  export default function RootLayout({ children }: { children: React.ReactNode }) {
    return (
      <html lang="en">
        <body className={inter.className}>
          <Providers>
            <Header />
            <main className="min-h-screen">{children}</main>
            <Footer />
          </Providers>
        </body>
      </html>
    );
  }
  ```

- [ ] `app/providers.tsx` - context providers:
  ```tsx
  'use client';

  import { ApiClient } from '@bookings/api-client';
  import { AuthManager } from '@bookings/auth-sdk';
  import { createContext, useContext, useMemo } from 'react';

  const ApiContext = createContext<ApiClient | null>(null);
  const AuthContext = createContext<AuthManager | null>(null);

  export function Providers({ children }: { children: React.ReactNode }) {
    const authManager = useMemo(
      () => new AuthManager(process.env.NEXT_PUBLIC_API_URL + '/auth'),
      []
    );

    const apiClient = useMemo(
      () => new ApiClient(process.env.NEXT_PUBLIC_API_URL, authManager),
      [authManager]
    );

    return (
      <AuthContext.Provider value={authManager}>
        <ApiContext.Provider value={apiClient}>
          {children}
        </ApiContext.Provider>
      </AuthContext.Provider>
    );
  }

  export const useApiClient = () => useContext(ApiContext)!;
  export const useAuth = () => useContext(AuthContext)!;
  ```

**2.3 Экран 1: Home Page (/) ✅**
- [ ] `app/page.tsx`:
  ```tsx
  import { SearchForm } from '@/components/SearchForm';
  import { FeaturedRooms } from '@/components/FeaturedRooms';
  import { HeroSection } from '@/components/HeroSection';

  export default function HomePage() {
    return (
      <>
        <HeroSection />
        <SearchForm />
        <FeaturedRooms />
      </>
    );
  }
  ```

- [ ] `components/SearchForm.tsx`:
  ```tsx
  'use client';

  import { useRouter } from 'next/navigation';
  import { useForm } from 'react-hook-form';
  import { zodResolver } from '@hookform/resolvers/zod';
  import { z } from 'zod';
  import { DatePicker, Input, Button } from '@bookings/ui-primitives';

  const searchSchema = z.object({
    checkIn: z.date(),
    checkOut: z.date(),
    adults: z.number().min(1).max(10),
    children: z.number().min(0).max(10),
  }).refine(data => data.checkOut > data.checkIn, {
    message: 'Check-out must be after check-in',
    path: ['checkOut']
  });

  type SearchFormData = z.infer<typeof searchSchema>;

  export function SearchForm() {
    const router = useRouter();
    const { register, handleSubmit, formState: { errors } } = useForm<SearchFormData>({
      resolver: zodResolver(searchSchema),
      defaultValues: {
        adults: 2,
        children: 0
      }
    });

    const onSubmit = (data: SearchFormData) => {
      const params = new URLSearchParams({
        checkIn: data.checkIn.toISOString().split('T')[0],
        checkOut: data.checkOut.toISOString().split('T')[0],
        adults: data.adults.toString(),
        children: data.children.toString(),
      });

      router.push(`/search?${params}`);
    };

    return (
      <form onSubmit={handleSubmit(onSubmit)} className="search-form">
        <DatePicker
          label="Check-in"
          {...register('checkIn')}
          error={errors.checkIn?.message}
        />
        <DatePicker
          label="Check-out"
          {...register('checkOut')}
          error={errors.checkOut?.message}
        />
        <Input
          label="Adults"
          type="number"
          {...register('adults', { valueAsNumber: true })}
          error={errors.adults?.message}
        />
        <Input
          label="Children"
          type="number"
          {...register('children', { valueAsNumber: true })}
          error={errors.children?.message}
        />
        <Button type="submit">Search Rooms</Button>
      </form>
    );
  }
  ```

**2.4 Экран 2: Search Results (/search) ✅**
- [ ] `app/search/page.tsx`:
  ```tsx
  import { Suspense } from 'react';
  import { SearchResults } from './SearchResults';
  import { Skeleton } from '@bookings/ui-primitives';

  export default function SearchPage() {
    return (
      <div className="container mx-auto px-4 py-8">
        <Suspense fallback={<Skeleton count={6} />}>
          <SearchResults />
        </Suspense>
      </div>
    );
  }
  ```

- [ ] `app/search/SearchResults.tsx`:
  ```tsx
  'use client';

  import { useSearchParams } from 'next/navigation';
  import { useQuery } from '@tanstack/react-query';
  import { RoomCard } from '@/components/RoomCard';
  import { Filters } from '@/components/Filters';
  import { useApiClient } from '@/app/providers';
  import type { Room } from '@bookings/types';

  export function SearchResults() {
    const searchParams = useSearchParams();
    const apiClient = useApiClient();

    const { data: rooms, isLoading } = useQuery({
      queryKey: ['rooms', searchParams.toString()],
      queryFn: () => apiClient.get<Room[]>(`/rooms/search?${searchParams}`)
    });

    if (isLoading) return <div>Loading...</div>;

    return (
      <div className="flex gap-8">
        <aside className="w-64">
          <Filters />
        </aside>

        <div className="flex-1">
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {rooms?.map(room => (
              <RoomCard key={room.id} room={room} />
            ))}
          </div>

          {rooms?.length === 0 && (
            <div className="text-center py-12">
              <p>No rooms found. Try adjusting your filters.</p>
            </div>
          )}
        </div>
      </div>
    );
  }
  ```

**2.5 Экран 3: Room Details (/rooms/[id]) ✅**
- [ ] `app/rooms/[id]/page.tsx`:
  ```tsx
  import { RoomGallery } from './RoomGallery';
  import { RoomInfo } from './RoomInfo';
  import { BookingPanel } from './BookingPanel';
  import { apiClient } from '@/lib/api';
  import type { Room } from '@bookings/types';

  export default async function RoomPage({ params }: { params: { id: string } }) {
    const room = await apiClient.get<Room>(`/rooms/${params.id}`);

    return (
      <div className="container mx-auto px-4 py-8">
        <RoomGallery images={room.images} />

        <div className="mt-8 grid grid-cols-1 lg:grid-cols-3 gap-8">
          <div className="lg:col-span-2">
            <RoomInfo room={room} />
          </div>

          <div className="lg:col-span-1">
            <BookingPanel room={room} />
          </div>
        </div>
      </div>
    );
  }
  ```

---

### PHASE 3: Guest Frontend - Booking Flow (Week 3-4)

**Цель:** Реализовать создание бронирования и оплату

#### Задачи:

**3.1 Экран 4: Booking Form (/booking/new) ✅**
- [ ] Форма с валидацией (react-hook-form + zod)
- [ ] Интеграция с `POST /api/bookings`
- [ ] Обработка промокодов
- [ ] Countdown timer (15 минут)
- [ ] Mock fallback если Booking Service не готов

**3.2 Экран 5: Payment Page (/booking/[id]/payment) ✅**
- [ ] Stripe Elements интеграция:
  ```tsx
  'use client';

  import { Elements } from '@stripe/react-stripe-js';
  import { loadStripe } from '@stripe/stripe-js';
  import { PaymentForm } from './PaymentForm';

  const stripePromise = loadStripe(process.env.NEXT_PUBLIC_STRIPE_PK!);

  // PAY-001: API контракт обновлён — требуется { bookingId, amount (cents), currency }
  // amount и currency должны приходить из booking details (booking-service)
  export function PaymentPage({ bookingId, amount, currency }: { bookingId: string; amount: number; currency: string }) {
    const { data: clientSecret } = useQuery({
      queryKey: ['payment-intent', bookingId],
      queryFn: async () => {
        const response = await apiClient.post('/payment/intent', { bookingId, amount, currency });
        return response.clientSecret;
      }
    });

    if (!clientSecret) return <div>Loading...</div>;

    return (
      <Elements stripe={stripePromise} options={{ clientSecret }}>
        <PaymentForm bookingId={bookingId} />
      </Elements>
    );
  }
  ```

- [ ] WebSocket listener для `PaymentAuthorized` event:
  ```tsx
  useEffect(() => {
    wsManager.on('BookingStatusChanged', (payload) => {
      if (payload.bookingId === bookingId && payload.status === 'Reserved') {
        router.push(`/booking/${bookingId}/confirmation`);
      }
    });

    return () => {
      wsManager.off('BookingStatusChanged');
    };
  }, [bookingId]);
  ```

**3.3 Экран 6: Confirmation Page ✅**
- [ ] Анимация успеха
- [ ] Отображение деталей бронирования
- [ ] "Download Receipt" (генерация PDF на клиенте - `jspdf`)
- [ ] "Add to Calendar" (ICS файл)

---

### PHASE 4: Guest Frontend - User Area (Week 4-5)

**Цель:** Профиль пользователя и история бронирований

#### Задачи:

**4.1 Экран 7: My Bookings (/my-bookings) ✅**
- [ ] Tabs (Upcoming, Past, Cancelled)
- [ ] Интеграция с `GET /api/bookings?userId=...`
- [ ] Фильтры по статусу
- [ ] Cancel booking modal
- [ ] Mock fallback

**4.2 Экран 8: User Profile (/profile) ✅**
- [ ] Profile form
- [ ] Avatar upload (на S3 или Cloudinary)
- [ ] Интеграция с `GET/PUT /api/guests/me`
- [ ] Logout button

**4.3 Auth Integration ✅**
- [ ] Google OAuth callback handling (`/auth/callback`)
- [ ] Protected routes (middleware):
  ```tsx
  // middleware.ts
  import { NextResponse } from 'next/server';
  import type { NextRequest } from 'next/server';

  export function middleware(request: NextRequest) {
    const token = request.cookies.get('refresh_token');

    if (!token) {
      return NextResponse.redirect(new URL('/login', request.url));
    }

    return NextResponse.next();
  }

  export const config = {
    matcher: ['/my-bookings', '/profile', '/booking/:path*']
  };
  ```

---

### PHASE 5: Payment Frontend (Week 5)

**Цель:** Standalone payment widget (может встраиваться в Guest Frontend)

**Технология:** React 19 + Vite

#### Задачи:

**5.1 Настроить Vite проект**
- [ ] Аналогично Guest Frontend, но без Next.js
- [ ] SPA режим
- [ ] Routing: react-router-dom

**5.2 Payment Widget**
- [ ] Переиспользовать код из Guest Frontend Phase 3
- [ ] Expose как embeddable widget:
  ```tsx
  // Может быть встроен в iframe или как React компонент
  <PaymentWidget bookingId="..." onSuccess={() => {}} />
  ```

---

### PHASE 6: Admin Frontend (Week 6-7)

**Цель:** CRUD интерфейс для управления номерами, бронированиями, ценами

**Технология:** Angular 19

#### Задачи:

**6.1 Настроить Angular проект**
- [ ] Angular Material установка
- [ ] Routing setup
- [ ] Auth guard
- [ ] HTTP interceptor для JWT

**6.2 Модули**
- [ ] Rooms Management:
  - List rooms (таблица с пагинацией)
  - Create/Edit room (форма)
  - Upload images
  - Manage amenities

- [ ] Bookings Management:
  - View all bookings (таблица с фильтрами)
  - View booking details
  - Cancel booking
  - Manual PMS confirmation (если автоматика не сработала)

- [ ] Pricing Management:
  - Price list (CRUD)
  - Seasonal rates
  - Promo codes (CRUD)

- [ ] Users Management:
  - List users
  - View user bookings
  - Ban/Unban user

**6.3 Dashboard**
- [ ] Overview stats (total bookings, revenue, occupancy rate)
- [ ] Charts (Chart.js или ngx-charts)
- [ ] Recent bookings widget

---

### PHASE 7: Notification Widget (Week 7)

**Цель:** Real-time уведомления (можно встроить в Guest Frontend)

**Технология:** React 19 + Vite

#### Задачи:

**7.1 Notification Center Component**
- [ ] Bell icon с badge (unread count)
- [ ] Dropdown список уведомлений
- [ ] WebSocket подписка на события:
  - BookingConfirmed
  - PaymentAuthorized
  - CheckInReminder
  - BookingCancelled

**7.2 Типы уведомлений**
- [ ] Toast notifications (react-hot-toast)
- [ ] In-app notification center
- [ ] Desktop notifications (Web Notifications API)

---

### PHASE 8: Testing & Optimization (Week 8)

**Цель:** Покрыть тестами, оптимизировать производительность

#### Задачи:

**8.1 Unit Tests**
- [ ] Components testing (Vitest + React Testing Library)
- [ ] Services testing
- [ ] Hooks testing
- [ ] Target coverage: >70%

**8.2 E2E Tests**
- [ ] Playwright setup
- [ ] Happy path scenario:
  ```typescript
  test('complete booking flow', async ({ page }) => {
    await page.goto('/');

    // Search
    await page.fill('[name="checkIn"]', '2026-02-15');
    await page.fill('[name="checkOut"]', '2026-02-20');
    await page.click('button:has-text("Search")');

    // Select room
    await page.click('.room-card >> nth=0');

    // Book
    await page.click('button:has-text("Book Now")');
    await page.fill('[name="firstName"]', 'John');
    await page.fill('[name="lastName"]', 'Doe');
    await page.fill('[name="email"]', 'john@example.com');
    await page.click('button:has-text("Proceed to Payment")');

    // Pay (with Stripe test card)
    await page.fill('[name="cardNumber"]', '4242424242424242');
    await page.fill('[name="cardExpiry"]', '12/25');
    await page.fill('[name="cardCvc"]', '123');
    await page.click('button:has-text("Pay Now")');

    // Confirmation
    await expect(page.locator('text=Booking Confirmed')).toBeVisible();
  });
  ```

**8.3 Performance Optimization**
- [ ] Next.js Image optimization
- [ ] Code splitting (dynamic imports)
- [ ] Bundle analysis (webpack-bundle-analyzer)
- [ ] Lighthouse audit (score >90)
- [ ] Web Vitals monitoring:
  - LCP < 2.5s
  - FID < 100ms
  - CLS < 0.1

**8.4 Accessibility Audit**
- [ ] WCAG 2.1 Level AA compliance
- [ ] Keyboard navigation
- [ ] Screen reader testing (NVDA, VoiceOver)
- [ ] Color contrast check

---

### PHASE 9: Deployment & CI/CD (Week 9)

**Цель:** Автоматизировать сборку и деплой

#### Задачи:

**9.1 Docker Images**
- [ ] Multi-stage Dockerfile для каждого frontend app:
  ```dockerfile
  # Guest Frontend Dockerfile
  FROM node:22-alpine AS builder
  WORKDIR /app

  COPY package.json pnpm-lock.yaml ./
  RUN corepack enable pnpm && pnpm install --frozen-lockfile

  COPY . .
  RUN pnpm build

  FROM node:22-alpine AS runner
  WORKDIR /app

  COPY --from=builder /app/.next ./.next
  COPY --from=builder /app/public ./public
  COPY --from=builder /app/package.json ./

  RUN corepack enable pnpm && pnpm install --prod --frozen-lockfile

  EXPOSE 3000
  CMD ["pnpm", "start"]
  ```

**9.2 Kubernetes Manifests**
- [ ] Deployments для каждого frontend
- [ ] Services
- [ ] Ingress (nginx-ingress):
  ```yaml
  apiVersion: networking.k8s.io/v1
  kind: Ingress
  metadata:
    name: frontend-ingress
    namespace: bookings
  spec:
    rules:
      - host: bookings.example.com
        http:
          paths:
            - path: /
              pathType: Prefix
              backend:
                service:
                  name: guest-frontend
                  port:
                    number: 3000

            - path: /admin
              pathType: Prefix
              backend:
                service:
                  name: admin-frontend
                  port:
                    number: 4200
  ```

**9.3 CI/CD Pipeline (GitHub Actions)**
- [ ] `.github/workflows/frontend-ci.yml`:
  ```yaml
  name: Frontend CI/CD

  on:
    push:
      branches: [main, develop]
      paths:
        - 'frontend/**'
    pull_request:
      branches: [main]

  jobs:
    test:
      runs-on: ubuntu-latest
      steps:
        - uses: actions/checkout@v4

        - uses: pnpm/action-setup@v2
          with:
            version: 9

        - uses: actions/setup-node@v4
          with:
            node-version: 22
            cache: 'pnpm'

        - name: Install dependencies
          run: pnpm install --frozen-lockfile

        - name: Lint
          run: pnpm lint

        - name: Type check
          run: pnpm type-check

        - name: Unit tests
          run: pnpm test

        - name: Build
          run: pnpm build

    e2e:
      runs-on: ubuntu-latest
      needs: test
      steps:
        - uses: actions/checkout@v4

        - name: Install Playwright
          run: pnpm exec playwright install --with-deps

        - name: Run E2E tests
          run: pnpm test:e2e

        - uses: actions/upload-artifact@v4
          if: always()
          with:
            name: playwright-report
            path: playwright-report/

    build-and-push:
      runs-on: ubuntu-latest
      needs: [test, e2e]
      if: github.ref == 'refs/heads/main'
      steps:
        - uses: actions/checkout@v4

        - name: Set up Docker Buildx
          uses: docker/setup-buildx-action@v3

        - name: Login to Container Registry
          uses: docker/login-action@v3
          with:
            registry: ghcr.io
            username: ${{ github.actor }}
            password: ${{ secrets.GITHUB_TOKEN }}

        - name: Build and push
          uses: docker/build-push-action@v5
          with:
            context: ./frontend/apps/guest-frontend
            push: true
            tags: ghcr.io/${{ github.repository }}/guest-frontend:latest
            cache-from: type=gha
            cache-to: type=gha,mode=max

    deploy:
      runs-on: ubuntu-latest
      needs: build-and-push
      steps:
        - name: Deploy to Kubernetes
          uses: azure/k8s-deploy@v4
          with:
            manifests: |
              kubernetes/guest-frontend.yaml
            images: |
              ghcr.io/${{ github.repository }}/guest-frontend:latest
            kubeconfig: ${{ secrets.KUBE_CONFIG }}
  ```

**9.4 Environment Setup**
- [ ] Staging environment (staging.bookings.example.com)
- [ ] Production environment (bookings.example.com)
- [ ] Feature preview deployments (Vercel-style)

---

## 4. Интеграция с Backend API

### 4.1 API Readiness Matrix

| API Endpoint | Service | Status | Mock Available | Integration Priority |
|--------------|---------|--------|----------------|---------------------|
| `POST /api/bookings` | Booking Service | ✅ Ready | ❌ No need | **P0 - Critical** |
| `GET /api/bookings/:id` | Booking Service | ✅ Ready | ❌ No need | **P0 - Critical** |
| `GET /api/bookings?userId=...` | Booking Service | ⚠️ Partial | ✅ Yes | **P1 - High** |
| `POST /api/bookings/:id/cancel` | Booking Service | ⚠️ Stub | ✅ Yes | **P1 - High** |
| `GET /api/rooms/search` | Room Service | ❌ Missing | ✅ Yes | **P0 - Critical** |
| `GET /api/rooms/:id` | Room Service | ❌ Missing | ✅ Yes | **P0 - Critical** |
| `GET /api/rooms/:id/availability` | Room Service | ❌ Missing | ✅ Yes | **P1 - High** |
| `POST /api/payment/intent` | Payment Service | ✅ Ready (PAY-001: контракт изменён — `{ bookingId, amount, currency }`) | ❌ No need | **P0 - Critical** |
| `POST /api/payment/webhook` | Payment Service | ⚠️ Ready, HMAC не реализован (PAY-002) | ❌ No need | **P0 - Critical** |
| `GET /api/guests/me` | Guest Service | ❌ Missing | ✅ Yes | **P2 - Medium** |
| `PUT /api/guests/me` | Guest Service | ❌ Missing | ✅ Yes | **P2 - Medium** |
| `GET /api/pricing/quote` | Pricing Service | ✅ Ready | ❌ No need | **P1 - High** |
| `GET /login` (OAuth) | Auth Service | ✅ Ready | ❌ No need | **P0 - Critical** |
| `GET /callback` | Auth Service | ✅ Ready | ❌ No need | **P0 - Critical** |
| `POST /validate-token` | Auth Service | ✅ Ready | ❌ No need | **P0 - Critical** |

### 4.2 Mock Implementation Strategy

**Для каждого missing API:**

```typescript
// packages/api-client/src/services/RoomService.ts
import type { Room, SearchParams } from '@bookings/types';
import { ApiClient } from '../client';
import { mockRooms } from '@bookings/mock-data';

export class RoomService {
  constructor(
    private client: ApiClient,
    private useMock: boolean = false
  ) {}

  async searchRooms(params: SearchParams): Promise<Room[]> {
    if (this.useMock) {
      // Simulate API delay
      await new Promise(resolve => setTimeout(resolve, 500));

      // Apply filters to mock data
      return mockRooms.filter(room => {
        if (params.minPrice && room.pricePerNight < params.minPrice) return false;
        if (params.maxPrice && room.pricePerNight > params.maxPrice) return false;
        if (params.capacity && room.capacity < params.capacity) return false;
        return true;
      });
    }

    return this.client.get<Room[]>('/rooms/search', { params });
  }

  async getRoom(id: string): Promise<Room> {
    if (this.useMock) {
      await new Promise(resolve => setTimeout(resolve, 300));
      const room = mockRooms.find(r => r.id === id);
      if (!room) throw new Error('Room not found');
      return room;
    }

    return this.client.get<Room>(`/rooms/${id}`);
  }
}
```

**Feature flag контроль:**

```typescript
// app/providers.tsx
const apiClient = useMemo(() => {
  const useMockRooms = !apiFeatures.roomService.enabled;
  const useMockGuests = !apiFeatures.guestService.enabled;

  return new ApiClient(
    process.env.NEXT_PUBLIC_API_URL,
    authManager,
    {
      rooms: useMockRooms,
      guests: useMockGuests,
      // ... остальные
    }
  );
}, [authManager]);
```

### 4.3 API Integration Checklist

**По мере готовности каждого сервиса:**

- [ ] Room Service готов:
  - [ ] Обновить `apiFeatures.roomService.enabled = true`
  - [ ] Протестировать интеграцию
  - [ ] Удалить mock код (опционально, можно оставить для dev mode)

- [ ] Guest Service готов:
  - [ ] Обновить feature flag
  - [ ] Протестировать профиль
  - [ ] Проверить GDPR endpoints (data export, deletion)

- [ ] Notification Service готов:
  - [ ] Подключить WebSocket endpoint
  - [ ] Протестировать real-time уведомления
  - [ ] Проверить email templates

---

## 5. Auth Flow архитектура

### 5.1 OAuth 2.0 + JWT Flow (с PKCE элементами)

```
┌─────────────┐         ┌──────────────┐         ┌────────────┐
│   Browser   │         │ Auth Service │         │   Google   │
└──────┬──────┘         └──────┬───────┘         └─────┬──────┘
       │                       │                       │
       │ 1. Click "Login"      │                       │
       │──────────────────────▶│                       │
       │                       │                       │
       │ 2. Redirect to Google │                       │
       │◀──────────────────────│                       │
       │                       │                       │
       │ 3. Google OAuth       │                       │
       │───────────────────────────────────────────────▶│
       │                       │                       │
       │ 4. User consents      │                       │
       │◀───────────────────────────────────────────────│
       │                       │                       │
       │ 5. Callback with code │                       │
       │──────────────────────▶│                       │
       │                       │ 6. Exchange code      │
       │                       │──────────────────────▶│
       │                       │ 7. User info          │
       │                       │◀──────────────────────│
       │                       │                       │
       │ 8. Access Token       │                       │
       │    (15 min, memory)   │                       │
       │    Refresh Token      │                       │
       │    (7 days, httpOnly) │                       │
       │◀──────────────────────│                       │
       │                       │                       │
       │ 9. API calls          │                       │
       │    + Bearer token     │                       │
       │──────────────────────▶│                       │
```

### 5.2 Token Management

**Access Token:**
- Хранение: Memory (React state/Zustand store)
- Lifetime: 15 минут
- Передача: `Authorization: Bearer <token>` header

**Refresh Token:**
- Хранение: httpOnly cookie (не доступен для JS → защита от XSS)
- Lifetime: 7 дней
- Передача: Автоматически с каждым запросом (cookie)

**Auto-refresh механизм:**
```typescript
// packages/auth-sdk/src/AuthManager.ts (продолжение)

private scheduleTokenRefresh(expiresIn: number) {
  // Обновляем за 1 минуту до истечения
  const refreshTime = (expiresIn - 60) * 1000;

  if (this.refreshTimer) clearTimeout(this.refreshTimer);

  this.refreshTimer = setTimeout(async () => {
    const success = await this.refreshToken();
    if (!success) {
      // Refresh failed → logout
      this.logout();
    }
  }, refreshTime);
}
```

### 5.3 Protected Routes (Next.js Middleware)

```typescript
// middleware.ts
import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';

const protectedRoutes = ['/my-bookings', '/profile', '/booking'];
const authRoutes = ['/login', '/auth/callback'];

export async function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl;
  const refreshToken = request.cookies.get('refresh_token');

  const isProtectedRoute = protectedRoutes.some(route => pathname.startsWith(route));
  const isAuthRoute = authRoutes.some(route => pathname.startsWith(route));

  // Если protected route и нет refresh token → redirect на login
  if (isProtectedRoute && !refreshToken) {
    return NextResponse.redirect(new URL('/login', request.url));
  }

  // Если auth route и есть refresh token → redirect на home
  if (isAuthRoute && refreshToken) {
    return NextResponse.redirect(new URL('/', request.url));
  }

  return NextResponse.next();
}

export const config = {
  matcher: ['/((?!api|_next/static|_next/image|favicon.ico).*)']
};
```

---

## 6. WebSocket интеграция

### 6.1 Real-time Events

**События от backend:**

| Event Type | Payload | UI Action |
|------------|---------|-----------|
| `BookingStatusChanged` | `{ bookingId, status, timestamp }` | Update booking status badge, show toast |
| `PaymentAuthorized` | `{ bookingId, amount }` | Redirect to confirmation page |
| `PaymentFailed` | `{ bookingId, reason }` | Show error message, allow retry |
| `BookingExpired` | `{ bookingId }` | Show expiration message, redirect |
| `CheckInReminder` | `{ bookingId, checkInDate }` | Show notification 24h before |

### 6.2 WebSocket Hook

```typescript
// hooks/useWebSocket.ts
import { useEffect } from 'react';
import { useWebSocketManager } from '@/app/providers';

export function useWebSocket(eventType: string, handler: (payload: any) => void) {
  const wsManager = useWebSocketManager();

  useEffect(() => {
    wsManager.on(eventType, handler);

    return () => {
      wsManager.off(eventType, handler);
    };
  }, [wsManager, eventType, handler]);
}

// Usage in component:
function BookingStatus({ bookingId }: { bookingId: string }) {
  const [status, setStatus] = useState<BookingStatus>('Pending');

  useWebSocket('BookingStatusChanged', (payload) => {
    if (payload.bookingId === bookingId) {
      setStatus(payload.status);
      toast.success(`Booking status: ${payload.status}`);
    }
  });

  return <Badge>{status}</Badge>;
}
```

### 6.3 Connection Management

```typescript
// app/providers.tsx
const WebSocketContext = createContext<WebSocketManager | null>(null);

export function Providers({ children }: { children: React.ReactNode }) {
  const authManager = useAuthManager();

  const wsManager = useMemo(() => {
    const manager = new WebSocketManager(
      process.env.NEXT_PUBLIC_WS_URL,
      () => authManager.getAccessToken()
    );

    // Auto-connect when authenticated
    if (authManager.isAuthenticated()) {
      manager.connect();
    }

    return manager;
  }, [authManager]);

  // Listen to auth state changes
  useEffect(() => {
    const handleAuthChange = (authenticated: boolean) => {
      if (authenticated) {
        wsManager.connect();
      } else {
        wsManager.disconnect();
      }
    };

    authManager.onAuthStateChange = handleAuthChange;

    return () => {
      authManager.onAuthStateChange = undefined;
    };
  }, [authManager, wsManager]);

  return (
    <WebSocketContext.Provider value={wsManager}>
      {children}
    </WebSocketContext.Provider>
  );
}
```

---

## 7. Design System сравнение

### 7.1 Shadcn/ui + Tailwind (Рекомендуется) ⭐

**Плюсы:**
- ✅ Copy-paste компоненты (не зависимость в node_modules)
- ✅ Полный контроль над кодом
- ✅ Radix UI под капотом (accessibility из коробки)
- ✅ Tailwind CSS (быстрая кастомизация)
- ✅ TypeScript first
- ✅ Dark mode support
- ✅ Работает с React, Next.js, Remix

**Минусы:**
- ❌ Нужно копировать компоненты вручную
- ❌ Обновления компонентов = ручная работа

**Компоненты:**
- Button, Input, Select, Dialog, Dropdown, Tabs, Card, Badge, Toast, Calendar, Date Picker, Form

**Setup:**
```bash
pnpm dlx shadcn-ui@latest init
pnpm dlx shadcn-ui@latest add button input select dialog
```

**Использование:**
```tsx
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';

<Button variant="default" size="lg">Book Now</Button>
<Input type="email" placeholder="Email" />
```

---

### 7.2 Chakra UI

**Плюсы:**
- ✅ Простой API
- ✅ Excellent accessibility
- ✅ Responsive design из коробки
- ✅ Theme customization
- ✅ Хорошая документация

**Минусы:**
- ❌ Больший bundle size (vs Shadcn)
- ❌ Меньше контроля (vs copy-paste подход)

**Компоненты:**
- 50+ components

**Setup:**
```bash
pnpm add @chakra-ui/react @emotion/react @emotion/styled framer-motion
```

**Использование:**
```tsx
import { Button, Input, ChakraProvider } from '@chakra-ui/react';

<ChakraProvider>
  <Button colorScheme="blue" size="lg">Book Now</Button>
  <Input placeholder="Email" />
</ChakraProvider>
```

---

### 7.3 Mantine

**Плюсы:**
- ✅ TypeScript first
- ✅ 100+ components
- ✅ Hooks library (useForm, useMediaQuery и т.д.)
- ✅ Excellent DX
- ✅ Dark theme из коробки

**Минусы:**
- ❌ Немного больше learning curve
- ❌ CSS-in-JS (может быть медленнее)

**Компоненты:**
- 100+ components + 40+ hooks

**Setup:**
```bash
pnpm add @mantine/core @mantine/hooks @mantine/form @mantine/dates
```

**Использование:**
```tsx
import { Button, TextInput, MantineProvider } from '@mantine/core';

<MantineProvider withGlobalStyles withNormalizeCSS>
  <Button size="lg">Book Now</Button>
  <TextInput placeholder="Email" />
</MantineProvider>
```

---

### 7.4 Рекомендация

**Для этого проекта: Shadcn/ui + Tailwind**

**Причины:**
1. Next.js отлично работает с Tailwind (zero-config)
2. Copy-paste подход → полный контроль
3. Radix UI → best-in-class accessibility
4. Маленький bundle size
5. Легко кастомизировать под бренд

**Альтернатива:** Mantine (если нужны готовые сложные компоненты типа DataTable, Rich Text Editor)

---

## 8. Критерии готовности

### 8.1 MVP Checklist (Phase 1-4)

**Функциональные требования:**
- [ ] Пользователь может найти номера по датам и количеству гостей
- [ ] Пользователь может просмотреть детали номера
- [ ] Пользователь может создать бронирование
- [ ] Пользователь может оплатить бронирование (Stripe Test Mode)
- [ ] Пользователь получает confirmation page после оплаты
- [ ] Пользователь может просмотреть свои бронирования
- [ ] Пользователь может редактировать профиль
- [ ] Google OAuth аутентификация работает

**Технические требования:**
- [ ] Все экраны responsive (mobile, tablet, desktop)
- [ ] Mock fallback для missing APIs
- [ ] Loading states (spinners, skeletons)
- [ ] Error handling (error boundaries, toast notifications)
- [ ] Form validation (react-hook-form + zod)
- [ ] Accessibility (keyboard navigation, ARIA labels)
- [ ] Unit tests coverage >70%
- [ ] E2E tests для happy path

**Performance:**
- [ ] Lighthouse score >85
- [ ] LCP <2.5s
- [ ] FID <100ms
- [ ] CLS <0.1

### 8.2 Production Checklist (Phase 5-9)

**Дополнительные функции:**
- [ ] Payment frontend standalone работает
- [ ] Admin frontend CRUD для rooms работает
- [ ] Admin frontend CRUD для bookings работает
- [ ] Notification widget real-time работает

**Infrastructure:**
- [ ] Docker images собираются
- [ ] Kubernetes deployment работает
- [ ] CI/CD pipeline настроен
- [ ] Staging environment развернут
- [ ] Production environment развернут

**Security:**
- [ ] HTTPS везде (в production)
- [ ] CSP headers настроены
- [ ] XSS защита
- [ ] CSRF защита
- [ ] Rate limiting
- [ ] Security headers (Helmet.js)

**Monitoring:**
- [ ] Error tracking (Sentry)
- [ ] Analytics (Google Analytics / Plausible)
- [ ] Web Vitals tracking
- [ ] User session recording (опционально, Hotjar/LogRocket)

---

## Timeline Summary

| Phase | Duration | Deliverables |
|-------|----------|--------------|
| Phase 0: Infrastructure | 1 week | Monorepo setup, project scaffolding |
| Phase 1: Shared Packages | 1-2 weeks | @bookings/* packages, mock data |
| Phase 2: Search & Browse | 1-2 weeks | Home, Search Results, Room Details |
| Phase 3: Booking Flow | 1-2 weeks | Booking Form, Payment, Confirmation |
| Phase 4: User Area | 1-2 weeks | My Bookings, Profile, Auth |
| Phase 5: Payment Widget | 1 week | Standalone payment frontend |
| Phase 6: Admin Frontend | 2 weeks | CRUD for rooms, bookings, pricing |
| Phase 7: Notification Widget | 1 week | Real-time notifications |
| Phase 8: Testing & Optimization | 1 week | Tests, performance, a11y |
| Phase 9: Deployment | 1 week | Docker, K8s, CI/CD |

**Total: ~10-12 weeks** для полного frontend (все микрофронтенды + admin)

**MVP (Phase 0-4): ~5-6 weeks** (только Guest Frontend с основным флоу)

---

## Приоритеты по микрофронтендам

**Если ресурсы ограничены, следовать такой очередности:**

1. **Guest Frontend (Next.js)** - P0 (критично для бизнеса)
   - Weeks 1-5: MVP готов
   - Week 6: Доработки после тестирования

2. **Shared Packages** - P0 (блокирует всё остальное)
   - Weeks 1-2: Параллельно с Guest Frontend

3. **Payment Frontend (React)** - P1 (можно встроить в Guest Frontend)
   - Week 5: Если нужен standalone widget

4. **Admin Frontend (Angular)** - P2 (можно делать после MVP)
   - Weeks 6-8

5. **Notification Widget** - P3 (nice to have)
   - Week 7

---

## Следующие шаги

1. ✅ **Frontend Roadmap создан**
2. ✅ **Backend: Room Service и Pricing Service реализованы** (по состоянию на 2026-02-02)
3. 🔧 **Начать Phase 0: Infrastructure** — инициализировать монорепо в `frontend/`
4. 🔧 **Добавить в API Gateway** маршруты `/api/rooms/**`, `/api/bookings/**`, `/api/pricing/**`
5. 🔧 **Завершить backend:** confirm/cancel в Booking Service, Guest Service, Notification Service
6. 🔧 **Week 2+: Phase 1 (Shared Packages) + Phase 2 (Search & Browse)** параллельно
7. 🔧 **Регулярные demo каждые 2 недели**

---

## Контакты и ресурсы

**Design System Links:**
- Shadcn/ui: https://ui.shadcn.com/
- Chakra UI: https://chakra-ui.com/
- Mantine: https://mantine.dev/

**Tools:**
- Stripe Test Cards: https://stripe.com/docs/testing
- React Query (TanStack Query): https://tanstack.com/query
- Zod: https://zod.dev/
- React Hook Form: https://react-hook-form.com/

---

**Документ актуален на: 2026-02-02**
**Обновлять после завершения каждой фазы**

---

Готовы начать? 🚀
