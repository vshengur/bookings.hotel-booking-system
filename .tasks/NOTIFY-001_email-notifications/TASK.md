# NOTIFY-001: Создать Notification Service с email-уведомлениями

## Статус: Open

## Описание
Пользователь не получает никаких уведомлений о статусе бронирования. Для MVP необходим сервис, который слушает MassTransit events и отправляет email-уведомления при ключевых событиях: создание бронирования, успешная оплата, подтверждение, отмена.

## Acceptance Criteria
- [ ] AC-1: Сервис notification-service запускается, подписан на RabbitMQ events через MassTransit
- [ ] AC-2: Consumer `BookingCreated` → отправляет email "Ваше бронирование создано, ожидается оплата"
- [ ] AC-3: Consumer `PaymentAuthorized` → отправляет email "Оплата получена"
- [ ] AC-4: Consumer `BookingConfirmed` (PmsConfirmed) → отправляет email "Бронирование подтверждено" с номером подтверждения
- [ ] AC-5: Consumer `BookingCancelled` → отправляет email "Бронирование отменено"
- [ ] AC-6: Consumer `BookingExpired` → отправляет email "Время оплаты истекло"
- [ ] AC-7: Все отправленные уведомления логируются в БД (NotificationLog)
- [ ] AC-8: Email provider конфигурируемый (для MVP — SMTP или SendGrid API)
- [ ] AC-9: HTML email templates для каждого типа уведомления
- [ ] AC-10: Сервис зарегистрирован в Consul, health check, Prometheus metrics, Dockerfile

## Функциональные требования
- FR-1: Технология: Go или C# — на усмотрение (рекомендую C# для consistency с MassTransit consumers в booking/payment)
- FR-2: MassTransit consumers для событий бронирования
- FR-3: Email template engine (Razor для C#, html/template для Go)
- FR-4: Database: NotificationLog (Id, Type, RecipientEmail, Subject, SentAt, Status, Error)
- FR-5: Retry при ошибке отправки (3 попытки с backoff)
- FR-6: Получение email гостя: через gRPC вызов к guest-service (или из payload события, если добавить)

## Нефункциональные требования
- NFR-1: Email отправляется в течение 30 секунд после события
- NFR-2: Failed notifications не блокируют pipeline (async, fire-and-forget с retry)

## Edge Cases / Corner Cases
- EC-1: Guest-service недоступен (нет email гостя) → залогировать, retry через 1 мин
- EC-2: SMTP server недоступен → retry с backoff, после 3 попыток — пометить Failed в NotificationLog
- EC-3: Дублирование events (MassTransit retry) → idempotency по eventId + bookingId + type
- EC-4: Email bounced → залогировать (для MVP не обрабатываем delivery status)

## Out of Scope
- SMS уведомления
- Push notifications
- Notification preferences (вкл/выкл по типам)
- Scheduled notifications (reminder за 24ч до check-in)
- REST API для notification history

## Dependencies
- MassTransit events от booking-service и payment-service
- Guest Service (GUEST-001) — для получения email
- SMTP сервер или SendGrid account

## Priority: P1

## Assignee: Кодер

## Связь с Roadmap
- IMPLEMENTATION_ROADMAP.md: секция 6.1–6.4
