using System.ComponentModel.DataAnnotations;

namespace Server.Models
{
    public class UpdateUserFullName
    {
        [Required]
        public string Name { get; set; }
    }
}
