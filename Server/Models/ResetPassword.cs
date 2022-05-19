using System.ComponentModel.DataAnnotations;

namespace Server.Models
{
    public class ResetPassword
    {
        [Required]
        public string Email { get; set; }
    }
}
