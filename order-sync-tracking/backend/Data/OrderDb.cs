using Microsoft.EntityFrameworkCore;
using OrderSync.Api.Models;

namespace OrderSync.Api.Data;

public class OrderDb(DbContextOptions<OrderDb> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<SyncAttempt> Attempts => Set<SyncAttempt>();
    public DbSet<OrderEvent> Events => Set<OrderEvent>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Order>().HasIndex(order => order.OrderNumber).IsUnique();
        model.Entity<Order>().HasMany(order => order.Attempts).WithOne()
            .HasForeignKey(attempt => attempt.OrderId).OnDelete(DeleteBehavior.Cascade);
        model.Entity<Order>().HasMany(order => order.Events).WithOne()
            .HasForeignKey(item => item.OrderId).OnDelete(DeleteBehavior.Cascade);
    }
}

// One gate is shared by all writes in this local application.
public class WriteGate
{
    public SemaphoreSlim Lock { get; } = new(1, 1);
}
