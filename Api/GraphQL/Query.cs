using Api.Data;
using Api.Domain;
using GreenDonut.Data;
using HotChocolate.Types.Pagination;

namespace Api.GraphQL;

[QueryType]
public static class Query
{
    [UsePaging]
    [UseFiltering]
    [UseSorting]
    public static async Task<Connection<Message>> GetMessages(
        PagingArguments pagingArgs,
        QueryContext<Message> query,
        MessageRepository repository,
        CancellationToken ct)
    {
        var page = await repository.GetMessagesAsync(pagingArgs, query, ct);
        return page.ToConnection();
    }
}
