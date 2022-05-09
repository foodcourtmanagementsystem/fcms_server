﻿using System.ComponentModel.DataAnnotations;

namespace Server.Models
{
    public class FoodItem
    {
        public long Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        [DataType(DataType.Text)]
        public string Description { get; set; }
        [Required]
        public string Image { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        [Required]
        public FoodCategory FoodCategory { get; set; }
    }
}