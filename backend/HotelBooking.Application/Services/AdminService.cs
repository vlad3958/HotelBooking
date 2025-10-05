using System.Collections.Generic;
using System.Threading.Tasks;
using HotelBooking.Application.Interfaces;
using HotelBooking.Domain;
using HotelBooking.Infrastructure.Interfaces; // Direct reference; consider abstraction later.
using HotelBooking.Application.DTO;

namespace HotelBooking.Application.Services;

public class AdminService : IAdminService
{
    private readonly IAdmin _adminRepo;
    public AdminService(IAdmin adminRepo) => _adminRepo = adminRepo;

    public Task<IEnumerable<Booking>> GetAllBookingsAsync() => Task.FromResult(_adminRepo.GetAllBookings());

    public Task<BookingStatsResponse> GetBookingStatsAsync(System.DateTime startDate, System.DateTime endDate)
    {
        var data = _adminRepo.GetBookingStatsData(startDate, endDate);
        var dto = new BookingStatsResponse
        {
            StartDate = data.StartDate,
            EndDate = data.EndDate,
            TotalBookings = data.TotalBookings,
            DistinctRooms = data.DistinctRooms,
            DistinctUsers = data.DistinctUsers,
            TotalRoomNights = data.TotalRoomNights
        };
        return Task.FromResult(dto);
    }
}
