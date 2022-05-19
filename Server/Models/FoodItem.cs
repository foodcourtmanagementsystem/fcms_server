using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models
{
    public class FoodItem
    {
        public long Id { get; set; }
        [Required]
        [StringLength(500)]
        public string Title { get; set; }
        [Required]
        [DataType(DataType.Text)]
        public string Description { get; set; }
        public double Price { get; set; } 
        [Required]
        public string Image { get; set; }
        
        public int Stock { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime? UpdatedAt { get; set; }

        [Required]
        [ForeignKey("FoodCategory")]
        public long FoodCategoryId { get; set; }

        public virtual FoodCategory? FoodCategory { get; set; }

    }
}
