using Api.Data;
using Api.Domain;

namespace Api.GraphQL;

// Makes Book a Relay Node: its `id` becomes a globally-unique, opaque ID, and the schema
// gets `node(id:)` / `nodes(ids:)` root fields. The [NodeResolver] decodes that global id
// back to an integer key and refetches the book.
[ObjectType<Book>]
public static partial class BookNode
{
    [NodeResolver]
    public static async Task<Book?> GetBookByIdAsync(
        int id,
        BookRepository repository,
        CancellationToken ct)
        => await repository.GetByIdAsync(id, ct);
}
