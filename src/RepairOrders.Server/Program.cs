// RepairOrders.Server: the BFF and the API in one app. Later it also serves the Angular build
// in production and proxies to `ng serve` in development.
using System.Text.Json.Serialization;
using RepairOrders.Server.Data;
using RepairOrders.Server.Api;

var builder = WebApplication.CreateBuilder(args);

// From ServiceDefaults: OpenTelemetry (logs, traces, metrics to the dashboard), health checks,
// service discovery, and retries/timeouts on outgoing HttpClient calls.
builder.AddServiceDefaults();

// EF Core for the "repairorders" database: reads ConnectionStrings:repairorders (from the AppHost's
// WithReference), pools DbContexts, retries brief SQL errors, and adds a health check and tracing.
builder.AddSqlServerDbContext<RepairOrdersDbContext>("repairorders",
    configureDbContextOptions: options =>
    {
        // Demo data in Development only. EF runs it after applying migrations; production never gets it.
        if (builder.Environment.IsDevelopment())
        {
            options.UseSeeding((context, _) => DevSeedData.Seed(context));
            options.UseAsyncSeeding((context, _, cancellationToken) => DevSeedData.SeedAsync(context, cancellationToken));
        }
    });

// YARP's forwarder, resolving names like `http://ui` through Aspire service discovery.
builder.Services.AddHttpForwarderWithServiceDiscovery();

// Enums go over the wire as text ("InProgress"), not numbers, matching how they're stored,
// so the API and the generated TypeScript types read clearly.
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

// From most specific to least: health, then the API, then "everything else goes to Angular."

// /health and /alive, mapped only in Development (see ServiceDefaults for why).
app.MapDefaultEndpoints();

// Everything under /api is the JSON API.
var api = app.MapGroup("/api");
api.MapRepairOrderEndpoints();

// Unknown API paths get a real 404. Without this they'd fall through to the Angular
// forwarder (dev) or index.html (production) and answer 200 with HTML.
// "/api/{**path}" beats the forwarder's "/{**catch-all}" because it starts with a literal segment.
api.Map("/{**path}", () => TypedResults.NotFound());

if (app.Environment.IsDevelopment())
{
    // Dev only: anything the server doesn't handle itself goes to the Angular dev server,
    // so the browser sees one origin. A catch-all route has the lowest priority, so real
    // endpoints (/health, /api/*) still win.
    app.MapForwarder("/{**catch-all}", "http://ui");
}

app.Run();
