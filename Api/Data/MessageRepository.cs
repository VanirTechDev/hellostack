using Api.Domain;
using GreenDonut.Data;

namespace Api.Data;

public sealed class MessageRepository(AppDbContext db)
{
    public async Task<Page<Message>> GetMessagesAsync(
        PagingArguments pagingArgs,
        QueryContext<Message> query,
        CancellationToken ct = default)
        => await db.Messages
            // .With() applies the client's filter, sort, and projection. The default-sort
            // configurator guarantees cursor pagination always has a key: fall back to
            // CreatedAt DESC when the client didn't sort, and always append Id as a unique
            // tiebreaker so cursors are stable.
            .With(query, sort => sort
                .IfEmpty(o => o.AddDescending(m => m.CreatedAt))
                .AddAscending(m => m.Id))
            .ToPageAsync(pagingArgs, ct);
}
