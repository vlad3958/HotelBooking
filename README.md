# HotelBooking

Застосунок для бронювання готелів: ASP.NET Core (.NET 9) + MySQL (EF Core / Pomelo) + Identity + JWT + статичний фронтенд (GitHub Pages) + Heroku (бекенд).

## Архітектура
```
backend/
  HotelBooking.Domain        (сутності: Hotel, Room, Booking, BookingStatsData)
  HotelBooking.Application   (DTO, сервіси, інтерфейси)
  HotelBooking.Infrastructure( DbContext, Repositories, Identity )
  HotelBooking.API           (Controllers, Program.cs, DI, CORS, JWT )
frontend/
  *.html + *.js (ES Modules, без фреймворків)
```

## Основні можливості
| Функція | Опис |
|---------|------|
| Реєстрація / Логін | JWT токен, зберігається у localStorage |
| Ролі | User, Admin (seed при старті або через env `Admin__Email` / `Admin__Password`) |
| Адмін панель | CRUD готелі / кімнати, перегляд бронювань, статистика (картки) |
| Бронювання | Запобігання перетину дат, вікно доступності кімнати |
| Статистика | totalBookings, distinctRooms, distinctUsers, totalRoomNights за діапазон |
| CORS | Гнучка конфігурація + ALLOW_ALL_CORS для діагностики |

## Швидкий старт (Dev)
Backend:
```powershell
cd backend/HotelBooking.API
dotnet restore
dotnet run --launch-profile https
```
Frontend (простий статичний сервер):
```powershell
cd frontend
npx http-server -p 5500
```
Відкрити: http://localhost:5500/login.html

## Початковий Admin
Через env змінні (рекомендовано в проді):
```powershell
Admin__Email=admin@yourdomain.com
Admin__Password=Str0ngPwd!23
```
Якщо не задано – створюється дефолт (переконайтесь що ви зміните його в продакшені).

## Авторизація
Усі захищені запити: заголовок
```
Authorization: Bearer <JWT>
```

## Frontend сторінки
| Сторінка | Призначення |
|----------|-------------|
| login.html | Логін / отримання JWT |
| hotels.html | Перегляд готелів + кімнат (публічно для авторизованих) |
| bookings.html | Бронювання поточного користувача |
| admin.html | Адмін панель (роль Admin) |

### Admin Panel (UI)
Включає:
- Картки статистики
- CRUD форми (готелі, кімнати)
- Перегляд усіх бронювань (JSON)

## Environment Variables
| Var | Призначення |
|-----|-------------|
| DATABASE_URL / CLEARDB_DATABASE_URL | MySQL conn string (парсинг mysql://) |
| JWT_SECRET_KEY | Підпис JWT (мін. 32 символи) |
| JWT_ISSUER / JWT_AUDIENCE | Параметри токена |
| Admin__Email / Admin__Password | Seed admin користувача |
| ALLOW_ALL_CORS | Якщо = `true` – тимчасово дозволити всі origin (для debug) |
| EXTRA_CORS_ORIGINS | Додаткові origin через `;` |
| PORT | Heroku port binding |
