var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume(); // data survives restarts
var appdb = postgres.AddDatabase("appdb");

var api = builder.AddProject<Projects.Api>("api")
    .WithReference(appdb)
    .WaitFor(appdb);

var web = builder.AddViteApp("web", "../web")
    .WithReference(api)
    .WaitFor(api);

builder.AddYarp("gateway")
    .WithConfiguration(yarp =>
    {
        // GraphQL (and its MCP/REST endpoints) -> the API.
        yarp.AddRoute("/graphql/{**catch-all}", api);
        // Everything else -> the Vite/React app (dev server now, static files in prod).
        yarp.AddRoute("/{**catch-all}", web);
    });

builder.Build().Run();
