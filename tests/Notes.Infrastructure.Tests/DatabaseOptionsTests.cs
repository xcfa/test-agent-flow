using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Notes.Infrastructure.Tests;

public sealed class DatabaseOptionsTests
{
    private static ServiceProvider Build(Dictionary<string, string?> settings)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(config);
        services.AddNotes();
        return services.BuildServiceProvider();
    }

    [Fact]
    public void Sqlite_WithoutConnectionString_FailsValidation()
    {
        using var sp = Build(new() { ["Database:Provider"] = "Sqlite" });

        Assert.Throws<OptionsValidationException>(() => sp.GetRequiredService<IOptions<Persistence.DatabaseOptions>>().Value);
    }

    [Fact]
    public void InMemory_WithoutConnectionString_Passes()
    {
        using var sp = Build(new() { ["Database:Provider"] = "InMemory" });

        Assert.Equal(Persistence.DatabaseProvider.InMemory, sp.GetRequiredService<IOptions<Persistence.DatabaseOptions>>().Value.Provider);
    }

    [Fact]
    public void Provider_IsCaseInsensitive()
    {
        using var sp = Build(new() { ["Database:Provider"] = "postgresql", ["Database:ConnectionString"] = "Host=x" });

        Assert.Equal(Persistence.DatabaseProvider.PostgreSql, sp.GetRequiredService<IOptions<Persistence.DatabaseOptions>>().Value.Provider);
    }

    [Fact]
    public void UnknownProvider_Throws()
    {
        using var sp = Build(new() { ["Database:Provider"] = "Oracle", ["Database:ConnectionString"] = "x" });

        Assert.ThrowsAny<Exception>(() => sp.GetRequiredService<IOptions<Persistence.DatabaseOptions>>().Value);
    }
}
