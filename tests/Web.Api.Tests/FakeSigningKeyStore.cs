using Web.Api.Infrastructure.Sessions;

namespace Web.Api.Tests;

public sealed class FakeSigningKeyStore(byte fill = 7) : ISigningKeyStore
{
    public Task<byte[]> GetKeyAsync(CancellationToken ct = default) => Task.FromResult(Enumerable.Repeat(fill, 64).ToArray());
}
