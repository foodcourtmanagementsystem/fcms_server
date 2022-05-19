using System.ComponentModel.DataAnnotations;

namespace Server.Models
{
    public class FoodCategory
    {
        public long Id { get; set; }
        [Required]
        [StringLength(200)]
        public string Title { get; set; }
        [Required]
        public string Image { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<FoodItem>? FoodItems { get; set; }
    }
}
