using Microsoft.AspNetCore.Http;

namespace Web.Api.Infrastructure.Sessions;

// Signed session tokens the site sends back as "Authorization: Bearer <token>"
public interface ISessionTokens
{
    Task<string> IssueAsync(SessionUser user, CancellationToken ct = default);

    // Null for a missing, forged or expired token
    Task<SessionUser?> ReadAsync(HttpRequest request, CancellationToken ct = default);
}
