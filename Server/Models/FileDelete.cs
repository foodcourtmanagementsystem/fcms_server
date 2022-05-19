using System.ComponentModel.DataAnnotations;

namespace Server.Models
{
    public class FileDelete
    {
        [Required]
        public string Path { get; set; }
    }
}
