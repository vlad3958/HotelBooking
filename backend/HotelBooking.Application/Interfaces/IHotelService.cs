using HotelBooking.Domain;
using System.Threading.Tasks;

namespace HotelBooking.Application.Interfaces;

public interface IHotelService
{
    Task<Hotel> AddHotelAsync(string name, string address, string description);
    Task RemoveHotelAsync(int hotelId);
    Task<Hotel> UpdateHotelAsync(int hotelId, string name, string address, string description);
}
