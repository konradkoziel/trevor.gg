using Lol.Shared.Riot;
using Lol.Shared.Riot.Models;

namespace Web.Api.Tests;

// Champions = null plays an unreachable Data Dragon; champion icons only when asked for
public sealed class FakeDataDragon(IReadOnlyList<Champion>? champions, bool championIcons = false) : IDataDragon
{
    public Task<string?> ChampionIconUrlAsync(string championName, CancellationToken ct = default) =>
        Task.FromResult<string?>(championIcons && champions is not null ? $"https://icons.test/{championName}.png" : null);

    public Task<string?> ProfileIconUrlAsync(int iconId, CancellationToken ct = default) =>
        Task.FromResult<string?>(champions is null ? null : $"https://icons.test/profile/{iconId}.png");

    public Task<string?> ItemIconUrlAsync(int itemId, CancellationToken ct = default) =>
        Task.FromResult<string?>(champions is null ? null : $"https://icons.test/item/{itemId}.png");

    public Task<IReadOnlyList<Champion>?> ChampionsAsync(CancellationToken ct = default) =>
        Task.FromResult(champions);
}
