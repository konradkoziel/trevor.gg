using Lol.Shared.Riot;
using Lol.Shared.Riot.Models;

namespace trevor.Tests;

public sealed class FakeDataDragon(string? iconUrl = "https://icons.test/Yasuo.png") : IDataDragon
{
    public string? RequestedChampion { get; private set; }

    public Task<string?> ChampionIconUrlAsync(string championName, CancellationToken ct = default)
    {
        RequestedChampion = championName;
        return Task.FromResult(iconUrl);
    }

    public Task<IReadOnlyList<Champion>?> ChampionsAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Champion>?>([]);
}
