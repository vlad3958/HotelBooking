using System.Collections.Generic;
using HotelBooking.Domain;

namespace HotelBooking.Infrastructure.Interfaces;

public interface IHotel
{
    Hotel AddHotel(string name, string address, string description);
    void RemoveHotel(int hotelId);
    Hotel UpdateHotel(int hotelId, string name, string address, string description);
}
