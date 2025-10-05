using System.Threading.Tasks;

namespace HotelBooking.Application.Interfaces;

public interface IRoomService
{
    Task AddRoomAsync(int hotelId, string name, double price, int capacity, DateTime? startDate, DateTime? endDate);
    Task RemoveRoomAsync(int roomId);
    Task UpdateRoomAsync(int roomId, string name, double price, int capacity);
}
