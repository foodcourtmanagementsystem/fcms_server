using System.ComponentModel.DataAnnotations;

namespace Server.Models
{
    public class ChangeEmail
    {
        [Required]
        [EmailAddress]
        public string NewEmail { get; set; }
    }
}
