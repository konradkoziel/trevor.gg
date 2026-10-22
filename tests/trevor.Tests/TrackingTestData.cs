using Lol.Shared.Riot.Models;

namespace trevor.Tests;

// A ranked 30-min match: Ann (puuid-1, Yasuo mid) and Bob (puuid-2) on team 100 with a random Amumu feeding 0/10/2;
// the enemy Teemo is even worse at 0/12/1
public static class TrackingTestData
{
    public static RiotMatchParticipant Player(
        string puuid, int teamId, bool win, int k, int d, int a, string champion = "Garen", string? position = "TOP",
        string? gameName = null, string? tag = null) =>
        new(puuid, champion, teamId, position, win, k, d, a, 150, 10, 15_000, 12, 9000, gameName, tag);

    public static RiotMatch Match(bool annWins = false, long duration = 1800, bool bobOnEnemyTeam = false) =>
        new(new RiotMatchInfo(duration, 420, "CLASSIC",
        [
            Player("puuid-1", 100, annWins, 2, 9, 3, "Yasuo", "MIDDLE"),
            Player("puuid-2", bobOnEnemyTeam ? 200 : 100, bobOnEnemyTeam ? !annWins : annWins, 7, 2, 10, "Thresh", "UTILITY"),
            Player("ally-1", 100, annWins, 5, 4, 6),
            Player("ally-2", 100, annWins, 3, 5, 8),
            Player("ally-3", 100, annWins, 0, 10, 2, "Amumu", "JUNGLE", "Random", "EUNE"),
            Player("enemy-1", 200, !annWins, 0, 12, 1, "Teemo", "TOP", "Nick", "EUNE"),
            Player("enemy-2", 200, !annWins, 15, 2, 4),
            Player("enemy-3", 200, !annWins, 6, 3, 9),
            Player("enemy-4", 200, !annWins, 4, 4, 7),
        ]));

    // An Arena game: every pair has the same TeamId, only PlayerSubteamId tells them apart. Ann (puuid-1) and
    // ally-1 are pair 1; pair 2 has the best and pair 3 the worst score of the lobby
    public static RiotMatch ArenaMatch() =>
        new(new RiotMatchInfo(1200, 1700, "CHERRY",
        [
            Player("puuid-1", 100, true, 6, 4, 3, "Yasuo", "") with { PlayerSubteamId = 1, SubteamPlacement = 2 },
            Player("ally-1", 100, true, 4, 5, 6, "Thresh", "", "Partner", "EUNE") with { PlayerSubteamId = 1, SubteamPlacement = 2 },
            Player("enemy-1", 100, true, 20, 1, 5) with { PlayerSubteamId = 2, SubteamPlacement = 1 },
            Player("enemy-2", 100, true, 15, 2, 5) with { PlayerSubteamId = 2, SubteamPlacement = 1 },
            Player("enemy-3", 200, false, 0, 15, 0) with { PlayerSubteamId = 3, SubteamPlacement = 3 },
            Player("enemy-4", 200, false, 1, 14, 1) with { PlayerSubteamId = 3, SubteamPlacement = 3 },
        ]));
}
