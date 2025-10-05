using Microsoft.AspNetCore.Mvc;
using HotelBooking.Application.Interfaces;
using System.Threading.Tasks;
using System;
using HotelBooking.Domain;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Claims;
using HotelBooking.Application.DTO;

namespace HotelBooking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "User,Admin")]
public class ClientController : ControllerBase
{
    private readonly IClientService _clientService;
    private readonly ILogger<ClientController> _logger;

    public ClientController(IClientService clientService, ILogger<ClientController> logger)
    {
        _clientService = clientService;
        _logger = logger;
    }

    [HttpGet("hotels")]
    public async Task<IActionResult> GetHotels()
    {
        try
        {
            var hotels = await _clientService.GetHotelsAsync();
            return Ok(hotels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching hotels");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("rooms")]
    public async Task<IActionResult> GetRooms()
    {
        try
        {
            var rooms = await _clientService.GetRoomsAsync();
            return Ok(rooms);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching rooms");
            return StatusCode(500, "Internal server error");
        }
    }
    [HttpGet("rooms/city/{city}")]
    public async Task<IActionResult> GetRoomByCity(string city)
    {
        try
        {
            var room = await _clientService.GetRoomByCityAsync(city);
            if (room == null) return NotFound();
            return Ok(room);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching room for city: {city}");
            return StatusCode(500, "Internal server error");
        }
    }
    [HttpGet("rooms/daterange")]
    public async Task<IActionResult> GetRoomsByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        if (endDate <= startDate)
            return BadRequest(new { error = "Кінцева дата повинна бути після початкової" });
        try
        {
            var rooms = await _clientService.GetRoomsByDateRangeAsync(startDate, endDate);
            return Ok(rooms);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching rooms for date range: {startDate} - {endDate}");
            return StatusCode(500, "Internal server error");
        }
    }
    [HttpPost("book")]
    [Authorize]
    public async Task<IActionResult> BookRoom([FromBody] BookingDto booking)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized(new { error = "Потрібно увійти в систему (login required)" });
        if (booking.EndDate <= booking.StartDate)
            return BadRequest(new { error = "EndDate must be after StartDate" });
        try
        {
            var booked = await _clientService.BookRoomAsync(userId, booking.RoomId, booking.StartDate, booking.EndDate);
            var response = new BookingResponse
            {
                Id = booked.Id,
                RoomId = booked.RoomId,
                UserId = booked.UserId,
                StartDate = booked.StartDate,
                EndDate = booked.EndDate,
                RoomName = booked.Room?.Name
            };
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error booking room for user: {userId}");
            return BadRequest(new { error = ex.Message });
        }
    }
    [HttpGet("bookings/me")]
    [Authorize]
    public async Task<IActionResult> GetMyBookings()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized(new { error = "Потрібно увійти в систему (login required)" });
        try
        {
            var bookings = await _clientService.GetUserBookingsAsync(userId);
            var response = bookings.Select(b => new BookingResponse
            {
                Id = b.Id,
                RoomId = b.RoomId,
                UserId = b.UserId,
                StartDate = b.StartDate,
                EndDate = b.EndDate,
                RoomName = b.Room?.Name
            });
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching bookings for user: {userId}");
            return StatusCode(500, "Internal server error");
        }
    }
}