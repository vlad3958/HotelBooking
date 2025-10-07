dotnet restore
dotnet run --launch-profile https
# HotelBooking

Полноцінний приклад застосунку для бронювання готелів: ASP.NET Core (.NET 9) + MySQL (EF Core / Pomelo) + Identity + JWT + статичний фронтенд (GitHub Pages) + Heroku (бекенд).

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

## Контроллери / Ендпоінти (коротко)
Auth (`/api/Auth`): register, login, admin/create-user, admin/status, admin/bootstrap (діагностика – видалити після налаштування).

Client (`/api/Client`): hotels, rooms, rooms/city/{city}, rooms/daterange, book, bookings/me.

Admin (`/api/Admin`): bookings, AddHotel, UpdateHotel/{id}, RemoveHotel/{id}, AddRoom, UpdateRoom/{id}, RemoveRoom/{id}, stats/bookings.

Формати дат: ISO8601 (наприклад `2025-10-05` або повний `2025-10-05T00:00:00Z`).

## Frontend сторінки
| Сторінка | Призначення |
|----------|-------------|
| login.html | Логін / отримання JWT |
| hotels.html | Перегляд готелів + кімнат (публічно для авторизованих) |
| bookings.html | Бронювання поточного користувача |
| admin.html | Адмін панель (роль Admin) |

### Admin Panel (UI)
Включає:
- Картки статистики (без “сирого” JSON)
- CRUD форми (готелі, кімнати)
- Перегляд усіх бронювань (JSON)
- Decode Token (debug)
- Toast-нотифікації успіх/помилка

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

### Приклад (Heroku)
```powershell
heroku config:set JWT_SECRET_KEY="<rand64>" JWT_ISSUER=HotelBookingAPI JWT_AUDIENCE=HotelBookingClient `
  Admin__Email=admin@yourdomain.com Admin__Password=Str0ngPwd!23 ALLOW_ALL_CORS=false -a <app-name>
```

## Міграції та БД
Автоматично виконується `Database.Migrate()` при старті. Ручні команди (якщо потрібно):
```powershell
dotnet ef migrations add <Name> --project backend/HotelBooking.Infrastructure --startup-project backend/HotelBooking.API
dotnet ef database update --project backend/HotelBooking.Infrastructure --startup-project backend/HotelBooking.API
```

## Деплой
1. Backend → Heroku: публікуємо `dotnet publish` і в Procfile вказуємо двійковий запуск.
2. Frontend → GitHub Pages (workflow збирає і деплоїть в `gh-pages`).
3. Налаштуйте CORS (ENV або оновити список у `Program.cs`).

## CORS
Базовий список + можливість:
- `ALLOW_ALL_CORS=true` (тільки тимчасово)
- `EXTRA_CORS_ORIGINS="https://mydomain1.com;https://mydomain2.com"`

## Безпека / Hardening
| Рекомендація | Пояснення |
|--------------|-----------|
| Змінити seed admin пароль | Дефолтні креденшали = ризик |
| Прибрати admin bootstrap/status | Після успішної ініціалізації ролей |
| Використати довгий JWT секрет | Мінімізує brute force |
| Вимкнути ALLOW_ALL_CORS у продакшені | Лише точні origins |
| Логи без паролів | Не логувати токени/паролі |

## Траблшутинг
| Симптом | Причина | Рішення |
|---------|---------|---------|
| 401 на admin endpoints | Немає ролі Admin або токен прострочений | Relogin / перевірити роль |
| Порожній список готелів | Ще не створено жодного | Додайте через admin панель |
| 404 /api/api/... | Подвійний префікс (виправлено) | Оновити кеш фронта |
| CORS помилки | Origin не в списку | EXTRA_CORS_ORIGINS або точна конфіг |

## План наступних покращень (IDEAS)
- Таблиця готелів у адмінці зі стрічковим редагуванням
- Пошук/фільтри кімнат
- Пагінація бронювань
- Role promotion UI (user -> admin)

## Swagger (опціонально в проді)
```csharp
if (app.Environment.IsDevelopment() || Environment.GetEnvironmentVariable("ENABLE_SWAGGER") == "1")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```
Тоді: `ENABLE_SWAGGER=1`.

## License
Internal / навчальний приклад (можна адаптувати під власні проекти).

