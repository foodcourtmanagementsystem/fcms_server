using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models
{
    public class OrderItem
    {
        public long Id { get; set; }

        [ForeignKey("FoodItem")]
        public long FoodItemId { get; set; }
        public virtual FoodItem? FoodItem { get; set; }


        [Range(1, Int32.MaxValue)]
        public int Quantity { get; set; }

        public bool IsCashOnDelivery { get; set; } = false;

        public bool IsProcessed { get; set; } 

        public bool IsCancelled { get; set; }

        public bool IsRefunded { get; set; }

        public string? PaymentId { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public string Status { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
        public string UserId { get; set; }
    }
}
