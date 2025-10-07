using HotelBooking.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
// DTO namespace removed – using domain entities directly for now.
using HotelBooking.Application.Interfaces;
using HotelBooking.Application.DTO;
namespace HotelBooking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdmin _adminService;
    private readonly ILogger<AdminController> _logger;
    private readonly IHotelService _hotelService;

    private readonly IRoomService _roomService;

    public AdminController(IAdmin adminService, ILogger<AdminController> logger, IHotelService hotelService, IRoomService roomService)
    {
        _adminService = adminService;
        _logger = logger;
        _hotelService = hotelService;
        _roomService = roomService;
    }

    [HttpGet("bookings")]
    public IActionResult GetAllBookings()
    {
        try
        {
            // Flatten the object graph to avoid deep navigation cycles (Hotel -> Rooms -> Bookings ...)
            var bookings = _adminService.GetAllBookings()
                .Select(b => new
                {
                    b.Id,
                    b.UserId,
                    b.RoomId,
                    b.StartDate,
                    b.EndDate,
                    Room = b.Room == null ? null : new
                    {
                        b.Room.Id,
                        b.Room.Name,
                        b.Room.Price,
                        b.Room.Capacity,
                        b.Room.StartDate,
                        b.Room.EndDate,
                        Hotel = b.Room.Hotel == null ? null : new
                        {
                            b.Room.Hotel.Id,
                            b.Room.Hotel.Name,
                            b.Room.Hotel.Address,
                            b.Room.Hotel.Description
                        }
                    }
                })
                .ToList();
            return Ok(bookings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all bookings");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("AddHotel")]
    public async Task<IActionResult> AddHotel(string name, string address, string description)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(address) || string.IsNullOrWhiteSpace(description))
            return BadRequest("Invalid hotel data.");

        try
        {
            var hotel = await _hotelService.AddHotelAsync(name, address, description);
            return Created($"/api/Admin/Hotels/{hotel.Id}", hotel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding hotel");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("UpdateHotel/{hotelId}")]
    public async Task<IActionResult> UpdateHotel(int hotelId, string name, string address, string description)
    {
        if (hotelId <= 0 || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(address) || string.IsNullOrWhiteSpace(description))
            return BadRequest("Invalid hotel data.");

        try
        {
            var updated = await _hotelService.UpdateHotelAsync(hotelId, name, address, description);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating hotel");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("RemoveHotel/{hotelId}")]
    public async Task<IActionResult> RemoveHotel(int hotelId)
    {
        if (hotelId <= 0)
            return BadRequest("Invalid hotel ID.");

        try
        {
            await _hotelService.RemoveHotelAsync(hotelId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing hotel");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("AddRoom")]
    public async Task<IActionResult> AddRoom(int hotelId, string name, double price, int capacity, DateTime? startDate, DateTime? endDate)
    {
        if (hotelId <= 0 || string.IsNullOrWhiteSpace(name) || price <= 0 || capacity <= 0)
            return BadRequest("Invalid room data.");

        if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            return BadRequest("StartDate cannot be after EndDate");

        try
        {
            await _roomService.AddRoomAsync(hotelId, name, price, capacity, startDate, endDate);
            return Ok("Room added successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding room");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("UpdateRoom/{roomId}")]
    public async Task<IActionResult> UpdateRoom(int roomId, string name, double price, int capacity)
    {
        if (roomId <= 0 || string.IsNullOrWhiteSpace(name) || price <= 0 || capacity <= 0)
            return BadRequest("Invalid room data.");

        try
        {
            await _roomService.UpdateRoomAsync(roomId, name, price, capacity);
            return Ok("Room updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating room");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("RemoveRoom/{roomId}")]
    public async Task<IActionResult> RemoveRoom(int roomId)
    {
        if (roomId <= 0)
            return BadRequest("Invalid room ID.");

        try
        {
            await _roomService.RemoveRoomAsync(roomId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing room");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("stats/bookings")]
    public async Task<IActionResult> GetBookingStats([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromServices] IAdminService adminService)
    {
        if (endDate <= startDate)
            return BadRequest(new { error = "EndDate must be after StartDate" });
        try
        {
            var dto = await adminService.GetBookingStatsAsync(startDate, endDate);
            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error computing booking stats");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}
