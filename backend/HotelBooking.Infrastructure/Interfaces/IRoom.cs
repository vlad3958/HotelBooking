using System.Collections.Generic;

namespace HotelBooking.Infrastructure.Interfaces;

public interface IRoom
{
    void AddRoom(int hotelId, string name, double price, int capacity, DateTime? startDate, DateTime? endDate);
    void RemoveRoom(int roomId);
    void UpdateRoom(int roomId, string name, double price, int capacity);
}
