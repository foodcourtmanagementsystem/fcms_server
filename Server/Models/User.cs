using Microsoft.AspNetCore.Identity;

namespace Server.Models
{
    public class User : IdentityUser
    {
        public string Name { get; set; }

        public UserAddress? UserAddress { get; set; }
    }
}
