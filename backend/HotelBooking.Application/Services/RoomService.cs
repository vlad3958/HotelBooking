using System.Threading.Tasks;
using HotelBooking.Application.Interfaces;
using HotelBooking.Infrastructure.Interfaces;

namespace HotelBooking.Application.Services;

public class RoomService : IRoomService
{
    private readonly IRoom _roomRepo;
    public RoomService(IRoom roomRepo) => _roomRepo = roomRepo;

    public Task AddRoomAsync(int hotelId, string name, double price, int capacity, DateTime? startDate, DateTime? endDate)
    {
        // Basic validation: if both provided ensure start <= end
        if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            throw new ArgumentException("StartDate cannot be after EndDate");

        _roomRepo.AddRoom(hotelId, name, price, capacity, startDate, endDate);
        return Task.CompletedTask;
    }
    public Task RemoveRoomAsync(int roomId) { _roomRepo.RemoveRoom(roomId); return Task.CompletedTask; }
    public Task UpdateRoomAsync(int roomId, string name, double price, int capacity) { _roomRepo.UpdateRoom(roomId, name, price, capacity); return Task.CompletedTask; }
}
