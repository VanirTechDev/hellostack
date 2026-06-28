namespace Api.Domain;

public sealed class Book
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Author { get; set; }
    /// <summary>Position in the main sequence; 0 for the prequel <em>New Spring</em>.</summary>
    public int SequenceNumber { get; set; }
    public int PublicationYear { get; set; }
    public int Pages { get; set; }
}
