#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Server.Models;


namespace Server.Data
{
    public class FcmsContext : IdentityDbContext<User>
    {
        public FcmsContext (DbContextOptions<FcmsContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
             base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<FoodCategory>()
            .HasIndex(fc => fc.Title)
            .IsUnique();

            modelBuilder.Entity<FoodItem>()
            .HasOne(fi => fi.FoodCategory)
            .WithMany(fc => fc.FoodItems)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        }

        

        public DbSet<Server.Models.UserAddress>? UserAddress { get; set; }

        

        public DbSet<Server.Models.FoodCategory>? FoodCategory { get; set; }

        

        public DbSet<Server.Models.FoodItem>? FoodItem { get; set; }

        

        public DbSet<Server.Models.CartItem>? CartItem { get; set; }

        public DbSet<Server.Models.OrderItem>? OrderItem { get; set; }


    }
}
