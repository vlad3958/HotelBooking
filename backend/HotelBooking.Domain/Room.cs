using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Domain
{
    public class Room
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Added default to avoid null issues
        public double Price { get; set; }
        public int Capacity { get; set; }
        // Availability window for when this room can be booked (optional business rule metadata)
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // FK to Hotel
        public int HotelId { get; set; }
        public Hotel? Hotel { get; set; }

        public List<Booking> Bookings { get; set; } = new();
    }
}
