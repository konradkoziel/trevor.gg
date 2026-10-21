using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lol.Shared.Riot;

public static class RiotServiceCollectionExtensions
{
    // Data Dragon is a CDN: a slow answer means it's down, and callers fall back to no icon
    private static readonly TimeSpan DataDragonTimeout = TimeSpan.FromSeconds(5);

    // How often the never-recycled handlers re-resolve DNS
    private static readonly TimeSpan ConnectionLifetime = TimeSpan.FromMinutes(5);

    // Riot API key, platform and clients. App settings: RIOT_API_KEY (optional), RIOT_PLATFORM (default eun1)
    public static IServiceCollection AddRiot(this IServiceCollection services)
    {
        services.AddOptions<RiotOptions>()
            .Configure<IConfiguration>((options, config) =>
            {
                options.ApiKey = config["RIOT_API_KEY"];
                var platform = config["RIOT_PLATFORM"];
                if (!string.IsNullOrWhiteSpace(platform))
                    options.Platform = platform.Trim();
            })
            .Validate(o => RiotRouting.IsKnownPlatform(o.Platform), "RIOT_PLATFORM must be a Riot platform id, e.g. eun1 or euw1")
            .ValidateOnStart();

        // Callers may be singletons, so these clients can live for the whole app: rotate connections
        // in the handler instead of relying on IHttpClientFactory's handler recycling
        services.AddHttpClient<IDataDragon, DataDragonClient>(client => client.Timeout = DataDragonTimeout)
            .ConfigurePrimaryHttpMessageHandler(LongLivedHandler)
            .SetHandlerLifetime(Timeout.InfiniteTimeSpan);
        services.AddHttpClient<IRiotApiClient, RiotApiClient>()
            .ConfigurePrimaryHttpMessageHandler(LongLivedHandler)
            .SetHandlerLifetime(Timeout.InfiniteTimeSpan)
            .AddStandardResilienceHandler(); // retries 429/5xx with back-off, per-attempt and total timeouts

        return services;
    }

    private static SocketsHttpHandler LongLivedHandler() => new() { PooledConnectionLifetime = ConnectionLifetime };
}
