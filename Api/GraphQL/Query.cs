using Api.Data;
using Api.Domain;
using GreenDonut.Data;
using HotChocolate.Types.Pagination;

namespace Api.GraphQL;

[QueryType]
public static class Query
{
    [UsePaging(IncludeTotalCount = true)]
    [UseFiltering]
    [UseSorting]
    public static async Task<Connection<Book>> GetBooks(
        PagingArguments pagingArgs,
        QueryContext<Book> query,
        BookRepository repository,
        CancellationToken ct)
    {
        var page = await repository.GetBooksAsync(pagingArgs, query, ct);
        return page.ToConnection();
    }
}
