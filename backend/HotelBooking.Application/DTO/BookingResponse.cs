namespace HotelBooking.Application.DTO;

public class BookingResponse
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? RoomName { get; set; }
}
