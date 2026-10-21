using Lol.Shared.Riot.Models;
using Web.Api.Features.Stats;

namespace Web.Api.Tests;

public static class StatsTestData
{
    public const string Puuid = "puuid-1";
    public static readonly DateTimeOffset Start = new(2026, 1, 1, 18, 0, 0, TimeSpan.Zero);

    // A 30-minute Solo/Duo game, the n-th in time order
    public static PlayedMatch Played(int n, string champion, bool win, int k, int d, int a, int cs = 180, int damage = 18000,
        long seconds = 1800, int queue = 420) =>
        new($"EUN1_{n}", champion, win, k, d, a, cs, damage, seconds, queue, Start.AddHours(n));

    public static RiotMatch RiotMatchOf(string champion, bool win, long endMs = 1767290400000, string puuid = Puuid) =>
        new(new RiotMatchInfo(1800, 420, "CLASSIC",
        [
            new RiotMatchParticipant(puuid, champion, 100, "MIDDLE", win, 5, 2, 7, 150, 30, 20000, 20, 12000),
            new RiotMatchParticipant("someone-else", "Ahri", 200, "MIDDLE", !win, 2, 5, 3, 140, 0, 15000, 15, 10000),
        ], endMs));
}
