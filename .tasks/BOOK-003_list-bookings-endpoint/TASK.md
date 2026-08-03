# BOOK-003: Добавить endpoint списка бронирований пользователя

## Статус: Open

## Описание
У пользователя нет возможности получить список своих бронирований. Фронтенд-страница "My Bookings" требует endpoint `GET /api/bookings` с фильтрацией по пользователю, статусу и пагинацией. Также нужен endpoint для редактирования бронирования до подтверждения.

## Acceptance Criteria
- [ ] AC-1: `GET /api/bookings?userId={id}&status={status}&page={n}&pageSize={n}` возвращает список бронирований с пагинацией
- [ ] AC-2: Response содержит: items[], totalCount, page, pageSize, totalPages
- [ ] AC-3: Фильтрация по статусу: можно передать один или несколько статусов (Confirmed, Cancelled, и т.д.)
- [ ] AC-4: Бронирования отсортированы по дате создания (новые первые)
- [ ] AC-5: Пользователь видит только свои бронирования (авторизация по userId из JWT)
- [ ] AC-6: Каждое бронирование в списке содержит: id, roomId, checkIn, checkOut, status, totalPrice, createdAt
- [ ] AC-7: Пагинация по умолчанию: page=1, pageSize=10, максимум pageSize=50
- [ ] AC-8: Покрыто unit-тестами

## Функциональные требования
- FR-1: Создать `GetUserBookingsQuery` и `GetUserBookingsQueryHandler`
- FR-2: Создать `BookingListDto` (краткая версия для списка)
- FR-3: Repository метод: `GetByUserIdAsync(userId, filter, pagination)`
- FR-4: Валидация: page >= 1, pageSize 1-50, userId из JWT token
- FR-5: Пустой результат → пустой массив (не 404)

## Нефункциональные требования
- NFR-1: Время ответа < 300ms (p95) при 1000 бронирований пользователя
- NFR-2: DB query использует индекс по (GuestId, CreatedAtUtc DESC)

## Edge Cases / Corner Cases
- EC-1: Пользователь без бронирований → { items: [], totalCount: 0 }
- EC-2: page > totalPages → пустой массив
- EC-3: pageSize > 50 → ограничить до 50 (без ошибки)
- EC-4: Невалидный userId формат → 400

## Out of Scope
- PUT /api/booking/{id} (редактирование) — отложено
- Полнотекстовый поиск по бронированиям

## Dependencies
- JWT authentication в gateway (GW-001) для получения userId

## Priority: P0

## Assignee: Кодер

## Связь с Roadmap
- IMPLEMENTATION_ROADMAP.md: секция 3.1, задачи "GET /api/bookings — список бронирований пользователя" и "pagination и filtering"
