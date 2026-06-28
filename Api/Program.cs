using Api.Data;
using Api.Domain;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Aspire service defaults: OpenTelemetry, health checks, resilience, service discovery.
builder.AddServiceDefaults();

// Aspire injects the connection string named "appdb".
builder.AddNpgsqlDbContext<AppDbContext>("appdb");
builder.Services.AddScoped<BookRepository>();

builder.Services
    .AddGraphQLServer()
    .AddGlobalObjectIdentification() // Relay node interface + node(id:) / nodes(ids:) fields
    .AddApiTypes() // source-generated from [QueryType] / [ObjectType<Book>]; named after the project ("Api")
    .AddPagingArguments() // bind first/after/... into the injected PagingArguments
    .AddFiltering()
    .AddSorting()
    // HotChocolate 16 enables cost analysis with a default max field cost of 1000; a paged
    // connection combined with filtering + sorting exceeds it, so raise the ceiling.
    .ModifyCostOptions(o => o.MaxFieldCost = 5000)
    .ModifyRequestOptions(o => o.IncludeExceptionDetails = builder.Environment.IsDevelopment());

var app = builder.Build();

app.MapDefaultEndpoints();

// Least-steps local DB: create + seed the Wheel of Time series once.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
    if (!await db.Books.AnyAsync())
    {
        db.Books.AddRange(WheelOfTime.Books);
        await db.SaveChangesAsync();
    }
}

app.MapGraphQL();

// Enables `dotnet run -- schema export --output schema.graphql`; runs the server normally otherwise.
await app.RunWithGraphQLCommandsAsync(args);

internal static class WheelOfTime
{
    // The Wheel of Time by Robert Jordan (finished by Brandon Sanderson, books 12-14).
    // SequenceNumber 0 is the prequel, New Spring. Page counts are approximate (US hardcover).
    public static readonly Book[] Books =
    [
        new() { Title = "New Spring",            Author = "Robert Jordan",     SequenceNumber = 0,  PublicationYear = 2004, Pages = 334 },
        new() { Title = "The Eye of the World",  Author = "Robert Jordan",     SequenceNumber = 1,  PublicationYear = 1990, Pages = 814 },
        new() { Title = "The Great Hunt",        Author = "Robert Jordan",     SequenceNumber = 2,  PublicationYear = 1990, Pages = 705 },
        new() { Title = "The Dragon Reborn",     Author = "Robert Jordan",     SequenceNumber = 3,  PublicationYear = 1991, Pages = 624 },
        new() { Title = "The Shadow Rising",     Author = "Robert Jordan",     SequenceNumber = 4,  PublicationYear = 1992, Pages = 981 },
        new() { Title = "The Fires of Heaven",   Author = "Robert Jordan",     SequenceNumber = 5,  PublicationYear = 1993, Pages = 963 },
        new() { Title = "Lord of Chaos",         Author = "Robert Jordan",     SequenceNumber = 6,  PublicationYear = 1994, Pages = 987 },
        new() { Title = "A Crown of Swords",     Author = "Robert Jordan",     SequenceNumber = 7,  PublicationYear = 1996, Pages = 882 },
        new() { Title = "The Path of Daggers",   Author = "Robert Jordan",     SequenceNumber = 8,  PublicationYear = 1998, Pages = 604 },
        new() { Title = "Winter's Heart",        Author = "Robert Jordan",     SequenceNumber = 9,  PublicationYear = 2000, Pages = 668 },
        new() { Title = "Crossroads of Twilight", Author = "Robert Jordan",    SequenceNumber = 10, PublicationYear = 2003, Pages = 822 },
        new() { Title = "Knife of Dreams",       Author = "Robert Jordan",     SequenceNumber = 11, PublicationYear = 2005, Pages = 784 },
        new() { Title = "The Gathering Storm",   Author = "Brandon Sanderson", SequenceNumber = 12, PublicationYear = 2009, Pages = 766 },
        new() { Title = "Towers of Midnight",    Author = "Brandon Sanderson", SequenceNumber = 13, PublicationYear = 2010, Pages = 843 },
        new() { Title = "A Memory of Light",     Author = "Brandon Sanderson", SequenceNumber = 14, PublicationYear = 2013, Pages = 909 },
    ];
}
