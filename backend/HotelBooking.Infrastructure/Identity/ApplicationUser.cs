using Microsoft.AspNetCore.Identity;

namespace HotelBooking.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public bool IsAdmin { get; set; }
}
