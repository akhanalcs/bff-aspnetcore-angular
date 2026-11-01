using Microsoft.EntityFrameworkCore;

namespace RepairOrders.Server.Data;

// Demo repair orders for local development, so the order list isn't empty on a fresh database.
// EF runs this after applying migrations; Program.cs registers it in Development only.
public static class DevSeedData
{
    // Sync version: `dotnet ef database update` calls this one.
    public static void Seed(DbContext context)
    {
        var orders = context.Set<RepairOrder>();
        // Seeding runs on every database update, so only add data to an empty table.
        if (orders.Any()) return;
        orders.AddRange(DemoOrders());
        context.SaveChanges();
    }

    // Async version: Database.MigrateAsync() calls this one. EF asks for both, with the same logic.
    public static async Task SeedAsync(DbContext context, CancellationToken cancellationToken)
    {
        var orders = context.Set<RepairOrder>();
        if (await orders.AnyAsync(cancellationToken)) return;
        orders.AddRange(DemoOrders());
        await context.SaveChangesAsync(cancellationToken);
    }

    // One order in each status, so the list and its status filter have something to show.
    private static RepairOrder[] DemoOrders()
    {
        // UTC, like every timestamp the server creates. The browser shows it in the user's own time zone.
        // (DateTimeOffset.Now would be the same instant with my laptop's +05:45, which means nothing to other users.)
        var now = DateTimeOffset.UtcNow;
        return
        [
            new() { UnitNumber = "4127", Complaint = "Brakes squealing at low speed", Status = RepairOrderStatus.Open, CreatedAt = now.AddHours(-2) },
            new() { UnitNumber = "5582", Complaint = "Driver-side mirror cracked", Status = RepairOrderStatus.Open, CreatedAt = now.AddHours(-5) },
            new() { UnitNumber = "3310", Complaint = "Check engine light on, rough idle", Status = RepairOrderStatus.InProgress, CreatedAt = now.AddDays(-1) },
            new() { UnitNumber = "2096", Complaint = "A/C blows warm air", Status = RepairOrderStatus.Closed, CreatedAt = now.AddDays(-3) },
            new() { UnitNumber = "4410", Complaint = "Trailer lights flicker", Status = RepairOrderStatus.Cancelled, CreatedAt = now.AddDays(-4) },
        ];
    }
}
