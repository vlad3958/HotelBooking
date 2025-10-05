using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Domain
{
   public class Booking
    {
        public int Id { get; set; }
    public string UserId { get; set; } = string.Empty; // FK to Identity ApplicationUser
        public int RoomId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

    // Navigation to Identity user intentionally omitted; can be added via shadow or separate query
        public Room? Room { get; set; }
    }
}
