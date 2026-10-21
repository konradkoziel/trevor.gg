using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Web.Api.Infrastructure.Sessions;

// HS256 JWTs signed with the key from ISigningKeyStore
public class SessionTokens(ISigningKeyStore keys, TimeProvider time) : ISessionTokens
{
    public const string Issuer = "trevor-web-api";
    public const string Audience = "trevor-web";

    // Long enough not to log friends out every evening, short enough that a leaked token dies on its own
    public static readonly TimeSpan Lifetime = TimeSpan.FromDays(7);

    private const string BearerPrefix = "Bearer ";
    private const string AvatarClaim = "avatar";

    private readonly JsonWebTokenHandler _handler = new();

    public async Task<string> IssueAsync(SessionUser user, CancellationToken ct = default)
    {
        var now = time.GetUtcNow().UtcDateTime;
        return _handler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = Issuer,
            Audience = Audience,
            IssuedAt = now,
            NotBefore = now,
            Expires = now + Lifetime,
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = user.DiscordId,
                [JwtRegisteredClaimNames.Name] = user.Name,
                [AvatarClaim] = user.AvatarUrl,
            },
            SigningCredentials = new SigningCredentials(await SecurityKeyAsync(ct), SecurityAlgorithms.HmacSha256),
        });
    }

    public async Task<SessionUser?> ReadAsync(HttpRequest request, CancellationToken ct = default)
    {
        var header = request.Headers.Authorization.ToString();
        if (!header.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
            return null;

        var result = await _handler.ValidateTokenAsync(header[BearerPrefix.Length..].Trim(), new TokenValidationParameters
        {
            ValidIssuer = Issuer,
            ValidAudience = Audience,
            IssuerSigningKey = await SecurityKeyAsync(ct),
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            LifetimeValidator = (notBefore, expires, _, _) => IsWithinLifetime(notBefore, expires),
        });
        if (!result.IsValid)
            return null;

        var claims = result.ClaimsIdentity;
        var id = claims.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return id is null
            ? null
            : new SessionUser(id, claims.FindFirst(JwtRegisteredClaimNames.Name)?.Value ?? id, claims.FindFirst(AvatarClaim)?.Value ?? "");
    }

    // Checked against TimeProvider instead of the wall clock, so tests can travel in time
    private bool IsWithinLifetime(DateTime? notBefore, DateTime? expires)
    {
        var now = time.GetUtcNow().UtcDateTime;
        return expires is { } exp && exp > now && (notBefore is not { } nbf || nbf <= now);
    }

    private async Task<SymmetricSecurityKey> SecurityKeyAsync(CancellationToken ct) => new(await keys.GetKeyAsync(ct));
}
