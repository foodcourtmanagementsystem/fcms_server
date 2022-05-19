using System.ComponentModel.DataAnnotations;

namespace Server.Models
{
    public class ConfirmResetPassword
    {
        [Required]
        public string NewPassword { get; set; }
    }
}
