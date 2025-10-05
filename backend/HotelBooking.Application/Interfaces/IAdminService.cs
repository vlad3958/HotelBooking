using System.Collections.Generic;
using System.Threading.Tasks;
using HotelBooking.Domain;

namespace HotelBooking.Application.Interfaces;

public interface IAdminService
{
    Task<IEnumerable<Booking>> GetAllBookingsAsync();
    Task<HotelBooking.Application.DTO.BookingStatsResponse> GetBookingStatsAsync(System.DateTime startDate, System.DateTime endDate);
}
