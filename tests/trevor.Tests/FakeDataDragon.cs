using trevor.Infrastructure.Riot;

namespace trevor.Tests;

public sealed class FakeDataDragon(string? iconUrl = "https://icons.test/Yasuo.png") : IDataDragon
{
    public Task<string?> ChampionIconUrlAsync(string championName, CancellationToken ct = default) =>
        Task.FromResult(iconUrl);
}
