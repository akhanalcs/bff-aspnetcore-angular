// Aspire AppHost: the model of the whole app. `aspire run` reads it, starts each resource,
// wires them together, and opens the dashboard.
var builder = DistributedApplication.CreateBuilder(args);

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
    .WithReference(ui);


builder.Build().Run();
