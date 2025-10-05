using System;
using System.Collections.Generic;
using D = HotelBooking.Domain;

namespace HotelBooking.Infrastructure.Interfaces;

public interface IClient
{
    IEnumerable<D.Hotel> GetHotels();
    IEnumerable<D.Room> GetRooms();
    D.Room? GetRoomByCity(string city);
    IEnumerable<D.Room> GetRoomsByDateRange(DateTime startDate, DateTime endDate);
    D.Booking BookRoom(string userId, int roomId, DateTime startDate, DateTime endDate);
    IEnumerable<D.Booking> GetUserBookings(string userId);
}
