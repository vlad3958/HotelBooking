using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HotelBooking.Domain;
using HotelBooking.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Infrastructure.Repositories
{
    public class ClientRepository : IClient
    {
        private readonly HotelBookingDbContext _dbContext;
        public ClientRepository(HotelBookingDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public IEnumerable<Hotel> GetHotels()
        {
            return _dbContext.Hotels
                .Include(h => h.Rooms)
                .ToList();
        }
        public IEnumerable<Room> GetRooms()
        {
            return _dbContext.Rooms.ToList();
        }
        public IEnumerable<Room> GetRoomsByHotelId(int hotelId)
        {
            return _dbContext.Rooms.Where(r => r.HotelId == hotelId).ToList();
        }
        public Room? GetRoomByCity(string city)
        {
            if (string.IsNullOrWhiteSpace(city)) return null;
            var pattern = $"%{city.Trim()}%";
            // Use EF.Functions.Like for server-side translation (case insensitive collation assumed; otherwise lower both sides)
            return _dbContext.Rooms
                .Where(r => r.Hotel != null && r.Hotel.Address != null && EF.Functions.Like(r.Hotel.Address, pattern))
                .FirstOrDefault();
        }
        public IEnumerable<Room> GetRoomsByDateRange(DateTime startDate, DateTime endDate)
        {
            return _dbContext.Rooms
                .Where(r =>
                    // No overlapping bookings
                    !r.Bookings.Any(b => b.StartDate < endDate && b.EndDate > startDate)
                    // Respect room availability window if defined
                    && (!r.StartDate.HasValue || startDate >= r.StartDate.Value)
                    && (!r.EndDate.HasValue || endDate <= r.EndDate.Value)
                )
                .ToList();
        }
    public Booking BookRoom(string userId, int roomId, DateTime startDate, DateTime endDate)
        {
            var room = _dbContext.Rooms.Find(roomId);
            if (room == null)
            {
                throw new Exception("Room not found");
            }

            // Enforce availability window
            if (room.StartDate.HasValue && startDate < room.StartDate.Value)
                throw new Exception("Requested start date is before room availability window");
            if (room.EndDate.HasValue && endDate > room.EndDate.Value)
                throw new Exception("Requested end date is after room availability window");

            var overlappingBooking = _dbContext.Bookings
                .FirstOrDefault(b => b.RoomId == roomId && b.StartDate < endDate && b.EndDate > startDate);
            if (overlappingBooking != null)
            {
                throw new Exception("Room is already booked for the selected date range");
            }

            var booking = new Booking
            {
                UserId = userId,
                RoomId = roomId,
                StartDate = startDate,
                EndDate = endDate
            };

            _dbContext.Bookings.Add(booking);
            _dbContext.SaveChanges();

            return booking;
        }
        public IEnumerable<Booking> GetUserBookings(string userId)
        {
            return _dbContext.Bookings
                .Include(b => b.Room)
                .Where(b => b.UserId == userId)
                .ToList();
        }

    }
}
