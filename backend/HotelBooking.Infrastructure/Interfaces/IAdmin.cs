using HotelBooking.Domain;
using System.Collections.Generic;

namespace HotelBooking.Infrastructure.Interfaces;

public interface IAdmin
{
    IEnumerable<Booking> GetAllBookings();
    IEnumerable<Booking> GetBookingsOverlapping(System.DateTime startDate, System.DateTime endDate);
    BookingStatsData GetBookingStatsData(System.DateTime startDate, System.DateTime endDate);
}
