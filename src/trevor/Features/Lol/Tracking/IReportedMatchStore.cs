namespace trevor.Features.Lol.Tracking;

// Matches that already got a card, so a friend whose match shows up later at Riot doesn't trigger a second one
public interface IReportedMatchStore
{
    Task<bool> ContainsAsync(string matchId, CancellationToken ct = default);

    Task AddAsync(string matchId, CancellationToken ct = default);
}
