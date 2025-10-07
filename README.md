# HotelBooking

## Стек
- Backend: .NET 9, ASP.NET Core, EF Core (MySQL/Pomelo), Identity + JWT
- Архітектура: Domain / Application / Infrastructure / API + Frontend (статичні HTML + ES Modules)

## Основне
- Ролі: Admin, User (сидяться при старті)  
- Авторизація: JWT (localStorage у фронті)  
- Сутності: Hotel, Room, Booking, BookingStatsData  
- Перевірка доступності: відсутність перекриття бронювань + вікно доступності кімнати  
- Статистика для Admin: кількість бронювань, унікальні користувачі, кімнати, room-nights  

## Запуск (dev)
Backend:
```
cd backend/HotelBooking.API
dotnet restore
dotnet run --launch-profile https
```
Frontend:
```
cd frontend
npx http-server -p 5500
```

## Конфіг
- Редагуйте `appsettings.Development.json` (рядок підключення до MySQL)
- Секрет JWT у конфігурації

## Логін
- Admin (вже є): email: admin@hotel.local / пароль: Admin123$

## API 

### Ендпоінти

AuthController (`api/auth`):
- POST `api/auth/register`  – реєстрація користувача (роль завжди User)
	- Body: `{ "email": string, "password": string }`
	- 200: `{ message, id, email }`
- POST `api/auth/admin/create-user` (Admin) – створити користувача з роллю User або Admin
	- Body: `{ email, password, role }`
	- 200: `{ message, id, email, role }`
- POST `api/auth/login`  – логін, повертає JWT
	- Body: `{ email, password }`
	- 200: `{ token }`

ClientController (`api/client`, Roles: User|Admin):
- GET `api/client/hotels` – список готелів з кімнатами
- GET `api/client/rooms` – всі кімнати
- GET `api/client/rooms/city/{city}` – кімнати за містом
- GET `api/client/rooms/daterange?startDate=YYYY-MM-DD&endDate=YYYY-MM-DD` – фільтр по діапазону дат
- POST `api/client/book` – забронювати кімнату
	- Body: `{ roomId, startDate, endDate }`
	- 200: `{ id, roomId, userId, startDate, endDate, roomName }`
- GET `api/client/bookings/me` – бронювання поточного користувача

AdminController (`api/admin`, Role: Admin):
- GET `api/admin/bookings` – всі бронювання
- POST `api/admin/AddHotel?name=&address=&description=` – додати готель (query/form params)
- POST `api/admin/UpdateHotel/{hotelId}?name=&address=&description=` – оновити готель
- DELETE `api/admin/RemoveHotel/{hotelId}` – видалити готель
- POST `api/admin/AddRoom?hotelId=&name=&price=&capacity=&startDate=&endDate=` – додати кімнату (startDate/endDate необов’язкові)
- POST `api/admin/UpdateRoom/{roomId}?name=&price=&capacity=` – оновити кімнату
- DELETE `api/admin/RemoveRoom/{roomId}` – видалити кімнату
- GET `api/admin/stats/bookings?startDate=YYYY-MM-DD&endDate=YYYY-MM-DD` – статистика бронювань за діапазон

### Авторизація
Передавайте заголовок:
`Authorization: Bearer <JWT>`

### Формати дат
Використовуйте ISO 8601 (`YYYY-MM-DD` або повний `YYYY-MM-DDTHH:MM:SSZ`).

## Frontend сторінки
- `login.html` – отримати токен
- `hotels.html` – перегляд готелів / бронювання
- `bookings.html` – мої бронювання

## Збірка / Публікація
- Публікація: `dotnet publish -c Release`
- Міграції (якщо додані): `dotnet ef migrations add <name>` / `dotnet ef database update`

## Environment Variables (Prod / Heroku / RDS)
Налаштуйте наступні змінні середовища перед запуском у production:

| Variable | Purpose | Notes |
|----------|---------|-------|
| `DATABASE_URL` | Основний рядок підключення (Heroku / Render) | Для Heroku з ClearDB ми автоматично парсимо `CLEARDB_DATABASE_URL` якщо `DATABASE_URL` відсутній |
| `CLEARDB_DATABASE_URL` | Авто-додається Heroku addon ClearDB | Формат `mysql://user:pass@host/db?reconnect=true` – парситься у `Program.cs` |
| `JWT_SECRET_KEY` | Секрет для підпису JWT | Використовуйте довгий випадковий рядок (мін. 32 символи) |
| `JWT_ISSUER` | Issuer в токені | Напр. `HotelBookingAPI` |
| `JWT_AUDIENCE` | Audience в токені | Напр. `HotelBookingClient` або origin фронтенду |
| `Admin__Email` | Email seed-адміна | Використовує подвійні підкреслення для вкладеної конфігурації |
| `Admin__Password` | Пароль seed-адміна | ОБОВʼЯЗКОВО змініть дефолтний |
| `ASPNETCORE_ENVIRONMENT` | Середовище | `Development` або `Production` |
| `PORT` | (Heroku) порт прослуховування | Автоматично підхоплюється у `Program.cs` |

### Приклад Heroku (PowerShell)
```powershell
heroku config:set JWT_SECRET_KEY="<random_64_chars>" JWT_ISSUER=HotelBookingAPI JWT_AUDIENCE=HotelBookingClient `
  Admin__Email=admin@yourdomain.com Admin__Password=Str0ngPwd!23 ASPNETCORE_ENVIRONMENT=Production
```

## Перехід з Heroku ClearDB на AWS RDS (MySQL)
Якщо ви найближчим часом переходите на RDS:
1. Створіть інстанс (Single-AZ, db.t3.micro, 20GB для тесту).
2. Увімкніть публічний доступ (або налаштуйте SSH/VPC доступ через бекенд).
3. Створіть користувача / базу (або використайте `admin` користувача з паролем).
4. Сформуйте рядок підключення у форматі:
	`Server=<endpoint>;Port=3306;Database=<dbname>;User Id=<user>;Password=<pwd>;SslMode=Preferred;CharSet=utf8mb4;`
5. Встановіть його як `DATABASE_URL` у середовищі (Heroku або інший хостинг).
6. Виконайте міграції:
```powershell
dotnet ef database update --project backend/HotelBooking.Infrastructure --startup-project backend/HotelBooking.API
```
	(Або покладіться на автоматичне `db.Database.Migrate()` при старті – це вже в коді.)
7. Після міграції – протестуйте логін / створення обʼєктів.

### Резервне копіювання
- RDS автоматично створює snapshots (якщо увімкнено при створенні). Для мінімальної ціни можна спершу вимкнути автоматичний backup, але це зменшує надійність.

### Оптимізація витрат
- Для dev оточення достатньо 20GB gp3 + Single-AZ.
- Вимикайте Enhanced Monitoring / Performance Insights якщо не потрібні.

## CORS Origins
Оновіть список у `Program.cs` коли будете знати домен фронтенду (S3 / CloudFront / інше).

## Swagger у Production
Зараз Swagger доступний лише у Development. Якщо треба тимчасово в проді – можна змінити умову:
```csharp
if (app.Environment.IsDevelopment() || Environment.GetEnvironmentVariable("ENABLE_SWAGGER") == "1")
{
	 app.UseSwagger();
	 app.UseSwaggerUI();
}
```
та задати `ENABLE_SWAGGER=1` як змінну середовища.

