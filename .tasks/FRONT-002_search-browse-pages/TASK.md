# FRONT-002: Guest Frontend — страницы поиска и просмотра номеров

## Статус: Open

## Описание
Реализовать первые 3 экрана Guest Frontend: Home (поиск), Search Results (список номеров), Room Details (детали номера). Это основа пользовательского пути — без этих страниц невозможно начать бронирование. При недоступности backend API — использовать mock data.

## Acceptance Criteria
- [ ] AC-1: Главная страница (`/`) с hero section и формой поиска (checkIn, checkOut, adults, children)
- [ ] AC-2: Страница результатов (`/search?checkIn=...&checkOut=...&adults=...`) с карточками номеров и фильтрами (цена, удобства)
- [ ] AC-3: Страница деталей номера (`/rooms/[id]`) с галереей, описанием, ценой и кнопкой "Book Now"
- [ ] AC-4: Форма поиска валидирует даты (checkIn >= today, checkOut > checkIn) через zod
- [ ] AC-5: Запросы к API через TanStack Query с loading skeletons и error states
- [ ] AC-6: Mock data fallback: если API недоступен, используется локальный JSON с 10+ номерами
- [ ] AC-7: Responsive design: корректно отображается на mobile (375px), tablet (768px), desktop (1440px)
- [ ] AC-8: Компоненты используют Shadcn/ui + Tailwind
- [ ] AC-9: Пагинация или infinite scroll на странице результатов
- [ ] AC-10: Accessibility: keyboard navigation, ARIA labels на формах и кнопках

## Функциональные требования
- FR-1: SearchForm component с react-hook-form + zod validation
- FR-2: RoomCard component (image, name, price, amenities, CTA)
- FR-3: Filters sidebar (price range slider, amenities checkboxes)
- FR-4: RoomGallery component (image carousel с полноэкранным просмотром)
- FR-5: BookingPanel (sticky sidebar с датами, ценой, кнопкой Book Now)
- FR-6: API integration: GET /api/rooms/search, GET /api/rooms/{id}, GET /api/rooms/{id}/availability
- FR-7: Mock data в packages/mock-data/ — 10-15 номеров с фото, ценами, удобствами

## Нефункциональные требования
- NFR-1: Lighthouse score > 85 (performance, accessibility, best practices)
- NFR-2: LCP < 2.5s, FID < 100ms, CLS < 0.1
- NFR-3: Optimized images (Next.js Image component)

## Edge Cases / Corner Cases
- EC-1: API недоступен → показать mock data с пометкой "Demo mode"
- EC-2: 0 результатов поиска → "No rooms found" с предложением изменить фильтры
- EC-3: Номер удалён между поиском и открытием деталей → 404 page
- EC-4: Очень длинное описание номера → truncate с "Read more"
- EC-5: Нет фото у номера → placeholder image

## Out of Scope
- Booking Form, Payment Page, Confirmation (следующая задача)
- Authentication flow
- My Bookings, Profile pages

## Dependencies
- FRONT-001 (монорепо) — должна быть выполнена
- Room Service API (уже готов) или mock data

## Priority: P1

## Assignee: Кодер

## Связь с Roadmap
- FRONTEND_ROADMAP.md: Phase 2 "Guest Frontend — Search & Browse", экраны 1-3
