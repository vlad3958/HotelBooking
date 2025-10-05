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
