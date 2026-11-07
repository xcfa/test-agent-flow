namespace Notes.Domain;

public sealed class Note
{
    public const int MaxTitleLength = 200;

    private Note(Guid id, string title, string body, DateTimeOffset createdAt)
    {
        Id = id;
        Title = title;
        Body = body;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid Id { get; }

    public string Title { get; private set; }

    public string Body { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static Note Create(string title, string body, DateTimeOffset now) =>
        new(Guid.NewGuid(), NormalizeTitle(title), body ?? string.Empty, now);

    public void Update(string title, string body, DateTimeOffset now)
    {
        Title = NormalizeTitle(title);
        Body = body ?? string.Empty;
        UpdatedAt = now;
    }

    private static string NormalizeTitle(string title)
    {
        var trimmed = title?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
        {
            throw new ArgumentException("Title is required.", nameof(title));
        }

        if (trimmed.Length > MaxTitleLength)
        {
            throw new ArgumentException($"Title must be at most {MaxTitleLength} characters.", nameof(title));
        }

        return trimmed;
    }
}
