using Notes.Domain;

namespace Notes.Domain.Tests;

public class NoteTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_TrimsTitle_AndSetsTimestamps()
    {
        var note = Note.Create("  Hello  ", "body", Now);

        Assert.Equal("Hello", note.Title);
        Assert.Equal("body", note.Body);
        Assert.Equal(Now, note.CreatedAt);
        Assert.Equal(Now, note.UpdatedAt);
        Assert.NotEqual(Guid.Empty, note.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_RejectsEmptyTitle(string title) =>
        Assert.Throws<ArgumentException>(() => Note.Create(title, "", Now));

    [Fact]
    public void Create_RejectsTooLongTitle() =>
        Assert.Throws<ArgumentException>(() => Note.Create(new string('x', Note.MaxTitleLength + 1), "", Now));

    [Fact]
    public void Update_ChangesContent_AndUpdatedAtOnly()
    {
        var note = Note.Create("a", "b", Now);
        var later = Now.AddMinutes(5);

        note.Update("c", "d", later);

        Assert.Equal("c", note.Title);
        Assert.Equal("d", note.Body);
        Assert.Equal(Now, note.CreatedAt);
        Assert.Equal(later, note.UpdatedAt);
    }
}
