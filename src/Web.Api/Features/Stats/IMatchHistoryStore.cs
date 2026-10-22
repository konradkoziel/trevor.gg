namespace Web.Api.Features.Stats;

public interface IMatchHistoryStore
{
    Task<IReadOnlyList<PlayedMatch>> ListAsync(string puuid, CancellationToken ct = default);

    Task SaveAsync(string puuid, IReadOnlyList<PlayedMatch> matches, CancellationToken ct = default);

    Task DeleteAsync(string puuid, IReadOnlyList<string> matchIds, CancellationToken ct = default);
}
