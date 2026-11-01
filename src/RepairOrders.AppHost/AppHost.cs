// Aspire AppHost: the model of the whole app. `aspire run` reads it, starts each resource,
// wires them together, and opens the dashboard.
var builder = DistributedApplication.CreateBuilder(args);

// SQL Server in a Docker container (image mssql/server:2022-latest).
// "sql" is the resource name: it's what the dashboard shows and what the container is named after. For eg: sql-5dba562a
var sqlServer = builder.AddSqlServer("sql")
    // Keep the container running after the AppHost stops, so the next run starts in seconds
    // instead of booting SQL Server again. Stop it from Docker Desktop if you need the memory.
    .WithLifetime(ContainerLifetime.Persistent)
    // Store the database files in a Docker volume, so data survives even if the container is removed.
    .WithDataVolume();

// A database on that server.
// - "repairorders" (first argument) is the resource name, and so the connection string name:
//   the server reads it as ConnectionStrings:repairorders.
// - databaseName is the real database created inside SQL Server. It defaults to the resource
//   name; it's written out here only to make the two names visible.
var db = sqlServer.AddDatabase("repairorders", databaseName: "repairorders");

// The Angular dev server, in development only. In production the Angular build is copied into
// the server's wwwroot, so there's no separate front-end app to deploy.
// runScriptName "start" → `npm run start` → the "start" script in src/ui/package.json (`ng serve`).
var ui = builder.AddJavaScriptApp("ui", "../ui", runScriptName: "start")
    // Runs `npm install` in src/ui before each start, so node_modules matches package.json
    // (and a fresh clone works). Node and npm themselves come from mise.
    .WithNpm()
    // `ng serve` listens on 4200 by default. Telling Aspire that port gives `ui` a URL in the
    // dashboard, and lets the server find it by name in 4c (YARP).
    .WithHttpEndpoint(targetPort: 4200);

// The BFF + API. "server" is the resource name in the dashboard and in service discovery.
var server = builder.AddProject<Projects.RepairOrders_Server>("server")
    // Only matters when deployed: gives the server a public URL (external ingress) in Container Apps.
    // Everything else stays internal; the browser only ever talks to the server (the BFF).
    .WithExternalHttpEndpoints()
    // Aspire polls /health
    .WithHttpHealthCheck("/health")
    // Gives the server ui's address (as config `services:ui:http:0`), so `http://ui` resolves
    // through service discovery. Used only by the dev-time forwarder in Program.cs.
    .WithReference(ui)
    // Hands the server the connection string as config `ConnectionStrings:repairorders`.
    .WithReference(db)
    // Don't start the server until SQL Server is up and the database exists.
    .WaitFor(db);


builder.Build().Run();
