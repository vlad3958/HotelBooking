using System.Threading.Tasks;
using HotelBooking.Application.Interfaces;
using HotelBooking.Domain;
using HotelBooking.Infrastructure.Interfaces;

namespace HotelBooking.Application.Services;

public class HotelService : IHotelService
{
    private readonly IHotel _hotelRepo;
    public HotelService(IHotel hotelRepo) => _hotelRepo = hotelRepo;

    public Task<Hotel> AddHotelAsync(string name, string address, string description) => Task.FromResult(_hotelRepo.AddHotel(name, address, description));
    public Task RemoveHotelAsync(int hotelId) { _hotelRepo.RemoveHotel(hotelId); return Task.CompletedTask; }
    public Task<Hotel> UpdateHotelAsync(int hotelId, string name, string address, string description) => Task.FromResult(_hotelRepo.UpdateHotel(hotelId, name, address, description));
}
