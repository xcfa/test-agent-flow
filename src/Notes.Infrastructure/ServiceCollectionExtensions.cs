using Microsoft.Extensions.DependencyInjection;
using Notes.Application;

namespace Notes.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNotes(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<INoteRepository, InMemoryNoteRepository>();
        services.AddScoped<NoteService>();
        return services;
    }
}
