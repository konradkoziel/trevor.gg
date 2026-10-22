using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Lol.Shared.Riot;

public static class RiotServiceCollectionExtensions
{
    // Data Dragon is a CDN: a slow answer means it's down, and callers fall back to no icon
    private static readonly TimeSpan DataDragonTimeout = TimeSpan.FromSeconds(5);

    // Name of the HttpClient behind DataDragonCdn
    private const string DataDragonHttpClient = "DataDragon";

    // How often the never-recycled handlers re-resolve DNS
    private static readonly TimeSpan ConnectionLifetime = TimeSpan.FromMinutes(5);

    // Riot API key, platform and clients. App settings: RIOT_API_KEY (optional), RIOT_PLATFORM (default eun1)
    public static IServiceCollection AddRiot(this IServiceCollection services)
    {
        services.AddOptions<RiotOptions>()
            .Configure<IConfiguration>((options, config) =>
            {
                // A key pasted with a trailing newline would be sent as is and rejected by Riot
                options.ApiKey = config["RIOT_API_KEY"]?.Trim();
                var platform = config["RIOT_PLATFORM"];
                if (!string.IsNullOrWhiteSpace(platform))
                    options.Platform = platform.Trim();
            })
            .Validate(o => RiotRouting.IsKnownPlatform(o.Platform), "RIOT_PLATFORM must be a Riot platform id, e.g. eun1 or euw1")
            .ValidateOnStart();

        // Data Dragon's cache needs a clock; an app that registered its own keeps it
        services.TryAddSingleton(TimeProvider.System);

        // Callers may be singletons, so these clients can live for the whole app: rotate connections
        // in the handler instead of relying on IHttpClientFactory's handler recycling
        services.AddHttpClient(DataDragonHttpClient, client => client.Timeout = DataDragonTimeout)
            .ConfigurePrimaryHttpMessageHandler(LongLivedHandler)
            .SetHandlerLifetime(Timeout.InfiniteTimeSpan);
        // A singleton, so its cache survives the per-invocation function classes of the website API
        services.AddSingleton(sp => ActivatorUtilities.CreateInstance<DataDragonCdn>(sp,
            sp.GetRequiredService<IHttpClientFactory>().CreateClient(DataDragonHttpClient)));
        services.AddSingleton<IDataDragon, DataDragonClient>();
        services.AddSingleton<IDataDragonCatalog, DataDragonCatalog>();
        services.AddHttpClient<IRiotApiClient, RiotApiClient>()
            .ConfigurePrimaryHttpMessageHandler(LongLivedHandler)
            .SetHandlerLifetime(Timeout.InfiniteTimeSpan)
            .AddStandardResilienceHandler(); // retries 429/5xx with back-off, per-attempt and total timeouts

        return services;
    }

    private static SocketsHttpHandler LongLivedHandler() => new() { PooledConnectionLifetime = ConnectionLifetime };
}
