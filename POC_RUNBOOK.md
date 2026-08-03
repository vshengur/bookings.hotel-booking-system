# Hotel Booking System — PoC Runbook

## Prerequisites

- Docker Desktop 4.x+ (с включённым Compose v2)
- Node.js 16+
- Git с инициализированными субмодулями: `git submodule update --init --recursive`

---

## 1. Первый запуск

### 1.1 Создать `.env`

```bash
cp .env.poc .env
```

Открыть `.env` и заполнить:
- `JWT_SECRET` — любая строка длиной 32+ символов
- `GOOGLE_CLIENT_ID` / `GOOGLE_CLIENT_SECRET` — из Google Cloud Console (OAuth 2.0)
- `GOOGLE_REDIRECT_URL` — оставить `http://localhost:8080/api/auth/callback`

### 1.2 Поднять весь бекенд

```bash
docker compose -f docker-compose.poc.yml up --build -d
```

Первый билд занимает 3–7 минут (скачивание SDK-образов, компиляция .NET и Go сервисов).

### 1.3 Проверить что всё запустилось

```bash
docker compose -f docker-compose.poc.yml ps
```

Все сервисы должны быть в статусе `Up` или `Up (healthy)`.

Полезные UI:
| Сервис | URL |
|---|---|
| Nginx gateway | http://localhost:8080 |
| RabbitMQ UI | http://localhost:15672 (guest/guest) |
| Consul UI | http://localhost:8500 |
| Seq (логи) | http://localhost:5341 |

### 1.4 Запустить frontend

#### Типичный старт для разработки / отладки

```bash
# Терминал 1 — бэкенд (если ещё не запущен)
docker compose -f docker-compose.poc.yml up -d

# Терминал 2 — фронтенд
cd frontend/booking-frontend
npm install        # только первый раз
npm run dev
```

Открыть http://localhost:5173

Vite dev server поддерживает горячую перезагрузку — изменения в `.tsx`/`.css` отражаются в браузере сразу без рестарта.

#### Переменные окружения фронтенда

Создать файл `frontend/booking-frontend/.env.local` (не коммитится):

```
# URL nginx gateway. Менять только при нестандартном порту.
VITE_GATEWAY_URL=http://localhost:8080

# Включить кнопку "Sign in" (Google OAuth).
# Оставить false пока Google credentials не настроены в .env.
# При false виден только "Demo mode" — достаточно для PoC.
VITE_GOOGLE_AUTH_ENABLED=false
```

#### Авторизация в UI

| Режим | Когда использовать |
|---|---|
| **Demo mode** | Всегда работает — устанавливает фиксированный `guestId` без OAuth |
| **Sign in** | Требует настроенных Google credentials в Consul и `VITE_GOOGLE_AUTH_ENABLED=true` |

#### Настройка Google OAuth (через Consul KV)

Credentials хранятся в Consul — **не** в `.env` и не в docker-compose.
Auth-service читает их из Consul при старте (приоритет: env → Consul).

**Шаг 1.** Создать OAuth 2.0 Client ID в [Google Cloud Console](https://console.cloud.google.com/):
- Application type: **Web application**
- Authorized redirect URI: `http://localhost:8080/api/auth/callback`

**Шаг 2.** Записать credentials в Consul (Consul должен быть запущен):

```bash
curl -X PUT -d "<CLIENT_ID>.apps.googleusercontent.com" \
  http://localhost:8500/v1/kv/config/GOOGLE_CLIENT_ID

curl -X PUT -d "<CLIENT_SECRET>" \
  http://localhost:8500/v1/kv/config/GOOGLE_CLIENT_SECRET
```

**Шаг 3.** Перезапустить auth-service:

```bash
docker compose -f docker-compose.poc.yml restart auth-service
```

**Шаг 4.** Включить кнопку Sign in в фронтенде — добавить в `frontend/booking-frontend/.env.local`:

```
VITE_GOOGLE_AUTH_ENABLED=true
```

**Проверить текущие значения в Consul:**

```bash
# CLIENT_ID
curl -s http://localhost:8500/v1/kv/config/GOOGLE_CLIENT_ID \
  | jq -r '.[0].Value' | base64 -d

# CLIENT_SECRET (маскируем вывод)
curl -s http://localhost:8500/v1/kv/config/GOOGLE_CLIENT_SECRET \
  | jq -r '.[0].Value' | base64 -d | cut -c1-6
```

**Удалить/перезаписать** — повторный `curl -X PUT` перезаписывает значение.

#### Альтернатива: запуск фронтенда внутри Docker Compose

Если нужно поднять фронтенд вместе с остальными сервисами одной командой,
добавить в `docker-compose.poc.yml`:

```yaml
frontend:
  image: node:20-alpine
  working_dir: /app
  volumes:
    - ./frontend/booking-frontend:/app
  command: sh -c "npm install && npm run dev -- --host 0.0.0.0"
  ports:
    - "5173:5173"
  networks: [booking-net]
```

После этого `docker compose -f docker-compose.poc.yml up -d` поднимет и фронтенд.
Hot reload работает через volume-маунт.

#### Производственный билд (без hot reload)

```bash
cd frontend/booking-frontend
npm run build        # результат в dist/
npm run preview      # локальный preview собранного dist/
```

Папку `dist/` можно раздать любым статическим сервером (nginx, Caddy и т.д.).
Для PoC demo достаточно `npm run dev`.

---

## 2. Demo flow (Happy Path)

Выполнять шаги последовательно — именно этот путь проверяет PoC end-to-end.

### Шаг 1: Авторизация

- Открыть http://localhost:5173
- Перейти по ссылке логина → http://localhost:8080/api/auth/login
- Авторизоваться через Google
- Получить JWT-токен (для PoC можно пропустить — токен пока не форсируется gateway)

### Шаг 2: Поиск комнат

- На главной странице ввести даты заезда и выезда
- Указать количество гостей
- Нажать **Search**
- Убедиться, что список комнат загрузился из room-service

### Шаг 3: Детали комнаты

- Выбрать комнату из результатов поиска
- Проверить описание, удобства, цену
- Нажать **Book this room**

### Шаг 4: Создание бронирования

- На странице подтверждения проверить даты и итоговую сумму
- Нажать **Proceed to payment**
- Убедиться, что в ответ пришёл `202 Accepted` с `bookingId`

### Шаг 5: Оплата (test mode)

- На странице оплаты убедиться, что payment intent создан (виден `intentId`)
- Нажать **Pay now (test mode)**
- Кнопка запускает симуляцию webhook (`POST /payment/webhook` со статусом `Succeeded`)
- Фронтенд ожидает пока статус брони изменится на `Reserved` или `Confirmed`

### Шаг 6: Подтверждение

- Страница `/confirmation/:bookingId` показывает финальный статус
- Статус `Reserved` — оплата принята, ждём PMS-подтверждения (симулятор ~30 сек)
- Статус `Confirmed` — бронирование полностью подтверждено

---

## 3. Проверка компонентов вручную (curl)

```bash
# Health gateway
curl http://localhost:8080/healthz

# Поиск комнат
curl "http://localhost:8080/api/rooms/search?checkIn=2026-06-01T00:00:00Z&checkOut=2026-06-07T00:00:00Z&adults=2&children=0"

# Создать бронирование
curl -X POST http://localhost:8080/api/booking \
  -H "Content-Type: application/json" \
  -d '{
    "bookingId": "00000000-0000-0000-0000-000000000099",
    "guestId": "00000000-0000-0000-0000-000000000001",
    "checkIn": "2026-06-01",
    "checkOut": "2026-06-07",
    "items": [{
      "roomId": 1,
      "adults": 2,
      "children": 0,
      "nights": 6,
      "pricePerNight": { "amount": 120, "currency": "EUR" }
    }]
  }'

# Получить бронирование
curl http://localhost:8080/api/booking/00000000-0000-0000-0000-000000000099

# Создать payment intent
curl -X POST http://localhost:8080/payment/intent \
  -H "Content-Type: application/json" \
  -d '{"bookingId": "00000000-0000-0000-0000-000000000099"}'

# Симулировать оплату
curl -X POST http://localhost:8080/payment/webhook \
  -H "Content-Type: application/json" \
  -d '{"bookingId": "00000000-0000-0000-0000-000000000099", "status": "Succeeded"}'
```

---

## 4. Остановить окружение

```bash
docker compose -f docker-compose.poc.yml down
# Удалить тома (полный сброс данных):
docker compose -f docker-compose.poc.yml down -v
```

---

## 5. Типичные проблемы

| Симптом | Решение |
|---|---|
| `bookings-service` не стартует | Проверить `bookings-db` healthy, RabbitMQ healthy. Посмотреть логи: `docker logs bookingservice.api` |
| Room search возвращает пустой список | Добавить тестовые данные: запустить `scripts/seed-rooms.sh` или добавить вручную через API |
| Payment intent возвращает ошибку gRPC | payment-service может не слушать gRPC на порту 80 — проверить `docker logs payment-service` |
| Статус брони не меняется после оплаты | Проверить RabbitMQ UI — есть ли события в очереди, нет ли ошибок в consumers |
| Consul не видит сервисы | room-service и pricing-service регистрируются сами, но для bookings-service регистрация Consul не настроена (не блокер для PoC) |

---

## 6. Seed данных (тестовые комнаты)

Если room-service запустился без данных, добавить несколько комнат:

```bash
curl -X POST http://localhost:8080/api/rooms \
  -H "Content-Type: application/json" \
  -d '{
    "room_number": "101",
    "room_type": "Standard",
    "floor": 1,
    "capacity": 2,
    "bed_type": "King",
    "size": 28,
    "description": "Comfortable standard room with city view"
  }'

curl -X POST http://localhost:8080/api/rooms \
  -H "Content-Type: application/json" \
  -d '{
    "room_number": "201",
    "room_type": "Deluxe",
    "floor": 2,
    "capacity": 3,
    "bed_type": "King",
    "size": 42,
    "description": "Spacious deluxe room with panoramic view"
  }'
```
