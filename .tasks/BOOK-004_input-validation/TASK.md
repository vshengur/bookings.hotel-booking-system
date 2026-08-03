# BOOK-004: Добавить валидацию при создании бронирования

## Статус: Open

## Описание
CreateBookingCommandHandler не проверяет корректность входных данных: даты могут быть в прошлом, checkOut может быть раньше checkIn, номер может быть недоступен. Без валидации система создаёт невалидные бронирования, которые затем fail-ят на этапе резервирования или оплаты.

## Acceptance Criteria
- [ ] AC-1: checkInDate >= today (UTC) — иначе 400 "Check-in date cannot be in the past"
- [ ] AC-2: checkOutDate > checkInDate — иначе 400 "Check-out must be after check-in"
- [ ] AC-3: Максимальная длительность бронирования 30 ночей — иначе 400
- [ ] AC-4: RoomId существует и номер доступен на указанные даты — иначе 409 Conflict
- [ ] AC-5: GuestId — valid GUID, не empty — иначе 400
- [ ] AC-6: Минимум 1 line item в бронировании — иначе 400
- [ ] AC-7: Adults >= 1 для каждого line item — иначе 400
- [ ] AC-8: Валидация PromoCode если передан (существует, активен, не истёк) — отложить детали на BOOK-005, здесь только формат
- [ ] AC-9: Все ошибки валидации возвращаются в стандартном формате ProblemDetails (RFC 7807)
- [ ] AC-10: Покрыто unit-тестами для каждого правила валидации

## Функциональные требования
- FR-1: Создать `CreateBookingCommandValidator` (FluentValidation)
- FR-2: Зарегистрировать validator в pipeline (MediatR ValidationBehavior)
- FR-3: Стандартный формат ошибок: `{ type, title, status, detail, errors: { field: ["message"] } }`

## Нефункциональные требования
- NFR-1: Валидация < 10ms (без внешних вызовов, проверка доступности — через BOOK-002)

## Edge Cases / Corner Cases
- EC-1: checkIn = checkOut (0 ночей) → 400
- EC-2: checkIn = today, время 23:59 UTC → допустимо (дата, не datetime)
- EC-3: Бронирование на 29 февраля невисокосного года → 400
- EC-4: Adults = 0, Children = 5 → 400 (минимум 1 adult)
- EC-5: Пустой body → 400 с перечислением всех обязательных полей

## Out of Scope
- Проверка capacity номера (вместимость vs количество гостей) — можно добавить позже
- Валидация промокода на уровне БД — задача BOOK-005

## Dependencies
- FluentValidation NuGet package
- MediatR ValidationBehavior pipeline

## Priority: P1

## Assignee: Кодер

## Связь с Roadmap
- IMPLEMENTATION_ROADMAP.md: секция 3.3 "Валидация при создании"
