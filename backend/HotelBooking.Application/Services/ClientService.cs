using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelBooking.Application.Interfaces;
using HotelBooking.Domain;
using HotelBooking.Infrastructure.Interfaces; // NOTE: Ideally introduce abstraction to avoid direct Infrastructure dependency.

namespace HotelBooking.Application.Services;

public class ClientService : IClientService
{
    private readonly IClient _clientRepo;

    public ClientService(IClient clientRepo) => _clientRepo = clientRepo;

    public Task<IEnumerable<Hotel>> GetHotelsAsync() => Task.FromResult(_clientRepo.GetHotels());
    public Task<IEnumerable<Room>> GetRoomsAsync() => Task.FromResult(_clientRepo.GetRooms());
    public Task<Room?> GetRoomByCityAsync(string city) => Task.FromResult(_clientRepo.GetRoomByCity(city));
    public Task<IEnumerable<Room>> GetRoomsByDateRangeAsync(DateTime startDate, DateTime endDate) => Task.FromResult(_clientRepo.GetRoomsByDateRange(startDate, endDate));
    public Task<Booking> BookRoomAsync(string userId, int roomId, DateTime startDate, DateTime endDate) => Task.FromResult(_clientRepo.BookRoom(userId, roomId, startDate, endDate));
    public Task<IEnumerable<Booking>> GetUserBookingsAsync(string userId) => Task.FromResult(_clientRepo.GetUserBookings(userId));
}
