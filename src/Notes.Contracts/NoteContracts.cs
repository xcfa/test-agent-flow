namespace Notes.Contracts;

public sealed record NoteDto(Guid Id, string Title, string Body, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);

public sealed record SaveNoteRequest(string Title, string Body);
