using Microsoft.EntityFrameworkCore;

namespace RepairOrders.Server.Data;

// The EF Core session for the repair-orders database. One instance per HTTP request (scoped).
// Only the options constructor: Aspire registers it with pooling, which reuses instances.
public class RepairOrdersDbContext(DbContextOptions<RepairOrdersDbContext> options) : DbContext(options)
{
    public DbSet<RepairOrder> RepairOrders => Set<RepairOrder>();

    // Column rules EF can't guess. Without lengths, every string would be nvarchar(max).
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RepairOrder>(order =>
        {
            order.Property(o => o.UnitNumber).HasMaxLength(20);
            order.Property(o => o.Complaint).HasMaxLength(500);
            // Store the status as text ("InProgress"), not a number, so the table reads clearly
            // and reordering the enum can't silently change what old rows mean.
            order.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
            // Maps to SQL Server's rowversion type and makes it the concurrency token.
            order.Property(o => o.RowVersion).IsRowVersion();
        });
    }
}
