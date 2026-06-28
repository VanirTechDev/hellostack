namespace Api.Domain;

public sealed class Message
{
    public int Id { get; set; }
    public required string Text { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
