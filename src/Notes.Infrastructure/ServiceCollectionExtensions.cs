using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Notes.Application;
using Notes.Infrastructure.Persistence;

namespace Notes.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNotes(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions<DatabaseOptions>()
            .BindConfiguration(DatabaseOptions.SectionName)
            .Validate(
                o => o.Provider == DatabaseProvider.InMemory || !string.IsNullOrWhiteSpace(o.ConnectionString),
                "Database:ConnectionString is required for the selected Database:Provider.")
            .ValidateOnStart();

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<InMemoryNoteRepository>();
        services.AddDbContext<NotesDbContext>((sp, b) =>
            DatabaseProviders.Configure(b, sp.GetRequiredService<IOptions<DatabaseOptions>>().Value));
        services.AddScoped<EfNoteRepository>();

        // Resolved lazily so test hosts can override the provider through configuration.
        services.AddScoped<INoteRepository>(sp =>
            sp.GetRequiredService<IOptions<DatabaseOptions>>().Value.Provider == DatabaseProvider.InMemory
                ? sp.GetRequiredService<InMemoryNoteRepository>()
                : sp.GetRequiredService<EfNoteRepository>());
        services.AddScoped<NoteService>();
        return services;
    }

    public static async Task InitializeNotesDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = services.GetRequiredService<IOptions<DatabaseOptions>>().Value;
        if (!options.MigrateOnStartup || options.Provider == DatabaseProvider.InMemory)
        {
            return;
        }

        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<NotesDbContext>();
        await db.Database.MigrateAsync(cancellationToken);
    }
}
