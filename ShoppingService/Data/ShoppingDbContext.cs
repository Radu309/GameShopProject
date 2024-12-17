using Microsoft.EntityFrameworkCore;
using ShoppingService.Models;

namespace ShoppingService.Data;

public class ShoppingDbContext : DbContext
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
            .HasForeignKey(o => o.UserId);

        // User - Cart (1-1)
        modelBuilder.Entity<Cart>()
            .HasOne(c => c.User)
            .WithOne(cl => cl.Cart)
            .HasForeignKey<Cart>(c => c.UserId);

        // Cart - CartItem (1-N)
        modelBuilder.Entity<CartItem>()
            .HasOne(ci => ci.Cart)
            .WithMany(c => c.CartItems)
            .HasForeignKey(ci => ci.CartId);

        // CartItem - Game (N-1)
        modelBuilder.Entity<CartItem>()
            .HasOne(ci => ci.Game)
            .WithMany(g => g.CartItems)
            .HasForeignKey(ci => ci.GameId);

        // Game - Review (1-N)
        modelBuilder.Entity<Review>()
            .HasOne(r => r.Game)
            .WithMany(g => g.Reviews)
            .HasForeignKey(r => r.GameId);

        // User - Review (1-N)
        modelBuilder.Entity<Review>()
            .HasOne(r => r.User)
            .WithMany(c => c.Reviews)
            .HasForeignKey(r => r.UserId);

        // Game - Image (1-N)
        modelBuilder.Entity<Image>()
            .HasOne(i => i.Game)
            .WithMany(g => g.Images)
            .HasForeignKey(i => i.GameId);

        base.OnModelCreating(modelBuilder);
    }
}