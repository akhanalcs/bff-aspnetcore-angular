// RepairOrders.Server: the BFF and the API in one app. Later it also serves the Angular build
// in production and proxies to `ng serve` in development.
using RepairOrders.Server.Data;

var builder = WebApplication.CreateBuilder(args);

// From ServiceDefaults: OpenTelemetry (logs, traces, metrics to the dashboard), health checks,
// service discovery, and retries/timeouts on outgoing HttpClient calls.
builder.AddServiceDefaults();

// EF Core for the "repairorders" database: reads ConnectionStrings:repairorders (from the AppHost's
// WithReference), pools DbContexts, retries brief SQL errors, and adds a health check and tracing.
builder.AddSqlServerDbContext<RepairOrdersDbContext>("repairorders");

// YARP's forwarder, resolving names like `http://ui` through Aspire service discovery.
builder.Services.AddHttpForwarderWithServiceDiscovery();

var app = builder.Build();

// /health and /alive, mapped only in Development (see ServiceDefaults for why).
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    // Dev only: anything the server doesn't handle itself goes to the Angular dev server,
    // so the browser sees one origin. A catch-all route has the lowest priority, so real
    // endpoints (/health, later /api/*) still win.
    app.MapForwarder("/{**catch-all}", "http://ui");
}

app.Run();
