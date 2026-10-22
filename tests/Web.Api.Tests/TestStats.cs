using Microsoft.Extensions.Options;
using Web.Api.Features.Stats;

namespace Web.Api.Tests;

public static class TestStats
{
    public static IOptions<MatchHistoryOptions> Options(int games = MatchHistoryOptions.DefaultGames) =>
        Microsoft.Extensions.Options.Options.Create(new MatchHistoryOptions { Games = games });
}
