using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Server.Models
{
    public class CartItem
    {
        public long Id { get; set; }

        [ForeignKey("FoodItem")]
        public long FoodItemId { get; set; }
        public virtual FoodItem? FoodItem { get; set; }

        [Range(1, Int32.MaxValue)]
        public int Quantity { get; set; }

        [JsonIgnore]
        public string? CartSessionId { get; set; }

    }
}
