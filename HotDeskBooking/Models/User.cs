using Microsoft.AspNetCore.Identity;

namespace HotDeskBooking.Models
{
    public class User : IdentityUser
    { 
        public string Name { get; set; }
        public bool IsAdmin { get; set; }
    }
}
