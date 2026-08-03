# BOOK-005: Обработка промокодов при бронировании

## Статус: Open

## Описание
Пользователь может ввести промокод при создании бронирования, но валидации и применения скидки нет. Необходимо создать таблицу промокодов, реализовать валидацию (существует, активен, не истёк, не превышен лимит использования) и применение скидки к итоговой стоимости бронирования.

## Acceptance Criteria
- [ ] AC-1: Таблица PromoCodes в БД: Code, DiscountType (Percent/Fixed), DiscountValue, ValidFrom, ValidTo, MaxUsages, CurrentUsages, IsActive
- [ ] AC-2: При создании бронирования с promoCode — валидация: код существует, активен, дата валидна, лимит не исчерпан
- [ ] AC-3: При валидном промокоде — скидка применяется к TotalPrice
- [ ] AC-4: Невалидный промокод → 400 с конкретной причиной (не найден / истёк / исчерпан)
- [ ] AC-5: CurrentUsages инкрементируется атомарно при использовании
- [ ] AC-6: CRUD API для промокодов (admin): POST/GET/PUT/DELETE /api/promo-codes

## Функциональные требования
- FR-1: Entity PromoCode с миграцией
- FR-2: PromoCodeValidator сервис
- FR-3: Применение скидки: Percent — % от суммы, Fixed — фиксированная сумма (но не ниже 0)
- FR-4: Логирование использования: PromoCodeUsage (PromoCodeId, BookingId, DiscountApplied, UsedAt)

## Edge Cases / Corner Cases
- EC-1: Промокод с скидкой 100% → TotalPrice = 0 (допускается ли?)
- EC-2: Fixed скидка больше TotalPrice → TotalPrice = 0
- EC-3: Два одновременных использования последнего промокода (race condition на CurrentUsages)
- EC-4: Промокод применён, но бронирование отменено → CurrentUsages откатывается?

## Priority: P2

## Assignee: Кодер

## Связь с Roadmap
- IMPLEMENTATION_ROADMAP.md: секция 3.4 "PromoCode обработка"
