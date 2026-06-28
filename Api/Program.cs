using Api.Data;
using Api.Domain;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Aspire service defaults: OpenTelemetry, health checks, resilience, service discovery.
builder.AddServiceDefaults();

// Aspire injects the connection string named "appdb".
builder.AddNpgsqlDbContext<AppDbContext>("appdb");
builder.Services.AddScoped<MessageRepository>();

builder.Services
    .AddGraphQLServer()
    .AddApiTypes() // source-generated from [QueryType] etc.; named after the project ("Api")
    .AddPagingArguments() // bind first/after/... into the injected PagingArguments
    .AddFiltering()
    .AddSorting()
    .ModifyRequestOptions(o => o.IncludeExceptionDetails = builder.Environment.IsDevelopment());

var app = builder.Build();

app.MapDefaultEndpoints();

// Least-steps local DB: create + seed once.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
    if (!await db.Messages.AnyAsync())
    {
        db.Messages.Add(new Message { Text = "Hello, World!!" });
        await db.SaveChangesAsync();
    }
}

app.MapGraphQL();

// Enables `dotnet run -- schema export --output schema.graphql`; runs the server normally otherwise.
await app.RunWithGraphQLCommandsAsync(args);
