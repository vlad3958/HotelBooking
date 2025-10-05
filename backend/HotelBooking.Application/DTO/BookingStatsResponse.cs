namespace HotelBooking.Application.DTO;

public class BookingStatsResponse
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalBookings { get; set; }
    public int DistinctRooms { get; set; }
    public int DistinctUsers { get; set; }
    public int TotalRoomNights { get; set; }
}
