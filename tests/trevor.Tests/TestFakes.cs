using trevor.Infrastructure.OpenAi;
using trevor.Infrastructure.Riot;

namespace trevor.Tests;

public sealed class FakeRiot : IRiotApiClient
{
    public bool IsConfigured { get; init; } = true;
    public RiotAccount? Account { get; init; } = new("puuid-1", "Player", "EUNE");
    public IReadOnlyList<LeagueEntry> Entries { get; init; } = [];
    public IReadOnlyList<string> MatchIds { get; init; } = ["EUN1_1"];
    public RiotMatch? Match { get; init; }
    public Exception? Error { get; init; }
    public RiotId? RequestedId { get; private set; }

    public Task<RiotAccount?> GetAccountAsync(RiotId riotId, CancellationToken ct = default)
    {
        RequestedId = riotId;
        return Error is null ? Task.FromResult(Account) : Task.FromException<RiotAccount?>(Error);
    }

    public Task<IReadOnlyList<LeagueEntry>> GetLeagueEntriesAsync(string puuid, CancellationToken ct = default) =>
        Task.FromResult(Entries);

    public Task<IReadOnlyList<string>> GetRecentMatchIdsAsync(string puuid, int count, CancellationToken ct = default) =>
        Task.FromResult(MatchIds);

    public Task<RiotMatch?> GetMatchAsync(string matchId, CancellationToken ct = default) =>
        Task.FromResult(Match);
}

public sealed class FakeAi(string? reply = "AI says hi") : ITrevorAi
{
    public string? LastInstruction { get; private set; }
    public string? LastInput { get; private set; }
    public int Calls { get; private set; }

    public Task<string?> ReplyAsync(string instruction, string input, int maxOutputTokens = 200, CancellationToken ct = default)
    {
        Calls++;
        LastInstruction = instruction;
        LastInput = input;
        return Task.FromResult(reply);
    }
}
