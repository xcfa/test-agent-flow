namespace Notes.Infrastructure.Persistence;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    public DatabaseProvider Provider { get; set; } = DatabaseProvider.InMemory;

    public string? ConnectionString { get; set; }

    public bool MigrateOnStartup { get; set; }
}
