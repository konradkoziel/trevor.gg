using Lol.Shared.Riot;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lol.Shared.Tests;

public class RiotServiceCollectionExtensionsTests
{
    private static ServiceProvider Services() => new ServiceCollection()
        .AddSingleton<IConfiguration>(new ConfigurationBuilder().Build())
        .AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance)
        .AddSingleton(typeof(ILogger<>), typeof(NullLogger<>))
        .AddRiot()
        .BuildServiceProvider();

    // The website API creates its function classes per request; the cache must not go with them
    [Fact]
    public void DataDragon_IsOneInstancePerApp()
    {
        using var services = Services();
        using var scope = services.CreateScope();

        Assert.Same(services.GetRequiredService<IDataDragon>(), scope.ServiceProvider.GetRequiredService<IDataDragon>());
        Assert.Same(services.GetRequiredService<IDataDragonCatalog>(), scope.ServiceProvider.GetRequiredService<IDataDragonCatalog>());
        Assert.Same(services.GetRequiredService<DataDragonCdn>(), scope.ServiceProvider.GetRequiredService<DataDragonCdn>());
    }
}
