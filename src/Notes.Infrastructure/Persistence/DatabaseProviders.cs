using Microsoft.EntityFrameworkCore;

namespace Notes.Infrastructure.Persistence;

internal static class DatabaseProviders
{
    // The only place with provider-specific connection code.
    private static readonly Dictionary<DatabaseProvider, Action<DbContextOptionsBuilder, string, string>> Use = new()
    {
        [DatabaseProvider.Sqlite] = (b, cs, asm) => b.UseSqlite(cs, o => o.MigrationsAssembly(asm)),
        [DatabaseProvider.SqlServer] = (b, cs, asm) => b.UseSqlServer(cs, o => o.MigrationsAssembly(asm)),
        [DatabaseProvider.PostgreSql] = (b, cs, asm) => b.UseNpgsql(cs, o => o.MigrationsAssembly(asm)),
    };

    public static string MigrationsAssembly(DatabaseProvider provider) => $"Notes.Migrations.{provider}";

    public static void Configure(DbContextOptionsBuilder builder, DatabaseOptions options)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(options);

        if (!Use.TryGetValue(options.Provider, out var use))
        {
            throw new InvalidOperationException($"Database provider '{options.Provider}' is not backed by EF Core.");
        }

        use(builder, options.ConnectionString ?? string.Empty, MigrationsAssembly(options.Provider));
    }
}
