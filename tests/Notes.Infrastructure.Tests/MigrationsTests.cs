using Microsoft.EntityFrameworkCore;
using Notes.Infrastructure.Persistence;

namespace Notes.Infrastructure.Tests;

public sealed class MigrationsTests
{
    [Theory]
    [InlineData(DatabaseProvider.Sqlite, "Data Source=unused.db")]
    [InlineData(DatabaseProvider.SqlServer, "Server=.;Database=unused")]
    [InlineData(DatabaseProvider.PostgreSql, "Host=localhost;Database=unused")]
    public void Migrations_MatchModel(DatabaseProvider provider, string connectionString)
    {
        using var db = TestDb.Create(new DatabaseOptions { Provider = provider, ConnectionString = connectionString });

        Assert.NotEmpty(db.Database.GetMigrations());
        Assert.False(db.Database.HasPendingModelChanges());
    }
}
