namespace Web.Api.Infrastructure.Sessions;

// The secret session tokens are signed with; the same for every instance of the API
public interface ISigningKeyStore
{
    Task<byte[]> GetKeyAsync(CancellationToken ct = default);
}
