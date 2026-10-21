using Lol.Shared.Riot;
using Lol.Shared.Riot.Models;

namespace Web.Api.Tests;

// Champions = null plays an unreachable Data Dragon
public sealed class FakeDataDragon(IReadOnlyList<Champion>? champions) : IDataDragon
{
    public Task<string?> ChampionIconUrlAsync(string championName, CancellationToken ct = default) =>
        Task.FromResult<string?>(null);

    public Task<IReadOnlyList<Champion>?> ChampionsAsync(CancellationToken ct = default) =>
        Task.FromResult(champions);
}
