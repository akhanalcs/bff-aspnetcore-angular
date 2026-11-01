using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using RepairOrders.Server.Data;

namespace RepairOrders.Server.Api;

// The repair-order API under /api/repair-orders. Returns DTOs, never entities.
public static class RepairOrderEndpoints
{
    public static RouteGroupBuilder MapRepairOrderEndpoints(this RouteGroupBuilder api)
    {
        var orders = api.MapGroup("/repair-orders");
        orders.MapGet("/", GetRepairOrders);
        return api;
    }

    // GET /api/repair-orders: newest first. Paging and filtering come with the list page (roadmap 4).
    private static async Task<Ok<RepairOrderListItem[]>> GetRepairOrders(
        RepairOrdersDbContext db, CancellationToken cancellationToken)
    {
        var items = await db.RepairOrders
            .OrderByDescending(o => o.CreatedAt)
            // Never return an unbounded list; real paging replaces this cap later.
            .Take(100)
            // Projecting into a DTO makes SQL select only these columns, and EF doesn't
            // track the results (no AsNoTracking needed).
            .Select(o => new RepairOrderListItem(o.Id, o.UnitNumber, o.Complaint, o.Status, o.CreatedAt))
            .ToArrayAsync(cancellationToken);

        return TypedResults.Ok(items);
    }
}

// One row of the order list. A DTO, so the API contract doesn't change when the table does.
public record RepairOrderListItem(
    int Id, string UnitNumber, string Complaint, RepairOrderStatus Status, DateTimeOffset CreatedAt);
