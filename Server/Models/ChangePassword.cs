using System.ComponentModel.DataAnnotations;

namespace Server.Models
{
    public class ChangePassword
    {
        [Required]
        public string OldPassword { get; set; }
        [Required]
        public string NewPassword { get; set; }
    }
}
