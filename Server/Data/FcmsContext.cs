#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace Server.Data
{
    public class FcmsContext : DbContext
    {
        public FcmsContext (DbContextOptions<FcmsContext> options)
            : base(options)
        {
        }

        public DbSet<Server.Models.FoodCategory> FoodCategory { get; set; }

        public DbSet<Server.Models.FoodItem> FoodItem { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
            .Entity<FoodCategory>()
            .HasMany(fc => fc.FoodItems)
            .WithOne(fi => fi.FoodCategory)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
