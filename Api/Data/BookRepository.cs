using Api.Domain;
using GreenDonut.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Data;

public sealed class BookRepository(AppDbContext db)
{
    /// <summary>Paged/filtered/sorted list for the `books` connection.</summary>
    public async Task<Page<Book>> GetBooksAsync(
        PagingArguments pagingArgs,
        QueryContext<Book> query,
        CancellationToken ct = default)
        => await db.Books
            // .With() applies the client's filter, sort, and projection. The default-sort
            // configurator guarantees cursor pagination always has a key: fall back to the
            // series sequence, and always append Id as a unique tiebreaker for stable cursors.
            .With(query, sort => sort
                .IfEmpty(o => o.AddAscending(b => b.SequenceNumber))
                .AddAscending(b => b.Id))
            .ToPageAsync(pagingArgs, ct);

    /// <summary>Single-book lookup used by the Relay node resolver.</summary>
    public async Task<Book?> GetByIdAsync(int id, CancellationToken ct = default)
        => await db.Books.FirstOrDefaultAsync(b => b.Id == id, ct);
}
