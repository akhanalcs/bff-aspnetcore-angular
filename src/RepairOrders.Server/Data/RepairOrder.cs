namespace RepairOrders.Server.Data;

// One repair order: a vehicle that came into the shop and what's wrong with it.
public class RepairOrder
{
    public int Id { get; set; }

    // The fleet's number for the vehicle, e.g. "4127".
    public string UnitNumber { get; set; } = "";

    // What the driver reported, e.g. "Brakes squealing". Stored as typed; Angular encodes it on display.
    public string Complaint { get; set; } = "";

    public RepairOrderStatus Status { get; set; } = RepairOrderStatus.Open;

    public DateTimeOffset CreatedAt { get; set; }

    // SQL Server bumps this on every update. EF adds it to the UPDATE's WHERE clause, so if someone
    // else changed the row first, zero rows match and EF throws, which the API turns into a 409.
    public byte[] RowVersion { get; set; } = [];
}

// Open → InProgress → Closed, or Cancelled. Orders are never deleted.
public enum RepairOrderStatus
{
    Open,
    InProgress,
    Closed,
    Cancelled
}
