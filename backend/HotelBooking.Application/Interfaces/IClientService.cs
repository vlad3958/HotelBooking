using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelBooking.Domain;

namespace HotelBooking.Application.Interfaces;

public interface IClientService
{
    Task<IEnumerable<Hotel>> GetHotelsAsync();
    Task<IEnumerable<Room>> GetRoomsAsync();
    Task<Room?> GetRoomByCityAsync(string city);
    Task<IEnumerable<Room>> GetRoomsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<Booking> BookRoomAsync(string userId, int roomId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<Booking>> GetUserBookingsAsync(string userId);
}