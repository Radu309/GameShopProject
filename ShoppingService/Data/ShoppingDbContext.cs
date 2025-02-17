﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ShoppingService.Models;

namespace ShoppingService.Data;

public class ShoppingDbContext : IdentityDbContext<User>
{
    public ShoppingDbContext(DbContextOptions<ShoppingDbContext> options)
        : base(options)
    {
    }
    public DbSet<User> Users { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Game> Games { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Image> Images { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User - Order (1-N)
        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // User - Cart (1-1)
        modelBuilder.Entity<User>()
            .HasOne(u => u.Cart)
            .WithOne(c => c.User)
            .HasForeignKey<Cart>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade); 

        // Cart - CartItem (1-N)
        modelBuilder.Entity<CartItem>()
            .HasOne(ci => ci.Cart)
            .WithMany(c => c.CartItems)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Restrict);
      
        // Order - CartItem (1-N)
        modelBuilder.Entity<CartItem>()
            .HasOne(ci => ci.Order)
            .WithMany(c => c.CartItems)
            .HasForeignKey(ci => ci.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // CartItem - Game (N-N)
        modelBuilder.Entity<CartItem>()
            .HasMany(ci => ci.Games)
            .WithMany(g => g.CartItems);

        // Game - Review (1-N)
        modelBuilder.Entity<Review>()
            .HasOne(r => r.Game)
            .WithMany(g => g.Reviews)
            .HasForeignKey(r => r.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        // User - Review (1-N)
        modelBuilder.Entity<Review>()
            .HasOne(r => r.User)
            .WithMany(c => c.Reviews)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Game - Image (1-N)
        modelBuilder.Entity<Image>()
            .HasOne(i => i.Game)
            .WithMany(g => g.Images)
            .HasForeignKey(i => i.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<User>().ToTable("User");
        modelBuilder.Entity<IdentityRole>().ToTable("_Roles");
        modelBuilder.Entity<IdentityUserRole<string>>().ToTable("_UserRoles");
        modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("_UserClaims");
        modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("_UserLogins");
        modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("_RoleClaims");
        modelBuilder.Entity<IdentityUserToken<string>>().ToTable("_UserTokens");
    }
}