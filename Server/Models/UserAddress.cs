using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models
{
    public class UserAddress
    {
        public long Id { get; set; }
        [Required]
        public long PhoneNumber { get; set; }
        [Required]
        public string Address1 { get; set; }
        public string? Address2 { get; set; }
        [Required]
        public int PinCode { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string State { get; set; }
        [Required]
        public string Country { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime CreatedAt { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("User")]
        public string? UserId { get; set; }
        public User? User { get; set; }
    }
}
