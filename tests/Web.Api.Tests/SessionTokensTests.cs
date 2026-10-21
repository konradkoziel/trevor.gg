using Microsoft.Extensions.Time.Testing;
using Web.Api.Infrastructure.Sessions;

namespace Web.Api.Tests;

public class SessionTokensTests
{
    private static readonly SessionUser Ann = new("80351110224678912", "Ann", "https://cdn.test/ann.png");

    [Fact]
    public async Task IssuedToken_ReadsBackAsTheSameUser()
    {
        var tokens = TestAuth.Tokens();

        var token = await tokens.IssueAsync(Ann);

        Assert.Equal(Ann, await tokens.ReadAsync(TestAuth.RequestWithBearer(token)));
    }

    [Fact]
    public async Task MissingOrMalformedHeader_IsNoUser()
    {
        var tokens = TestAuth.Tokens();

        Assert.Null(await tokens.ReadAsync(TestAuth.RequestWithBearer(null)));
        Assert.Null(await tokens.ReadAsync(TestAuth.RequestWithBearer("not-a-jwt")));
    }

    [Fact]
    public async Task TokenSignedWithAnotherKey_IsRejected()
    {
        var token = await TestAuth.Tokens(keyFill: 1).IssueAsync(Ann);

        Assert.Null(await TestAuth.Tokens(keyFill: 2).ReadAsync(TestAuth.RequestWithBearer(token)));
    }

    [Fact]
    public async Task TamperedToken_IsRejected()
    {
        var tokens = TestAuth.Tokens();
        var token = await tokens.IssueAsync(Ann);

        // Flip one character of the payload: someone trying to become another user
        var parts = token.Split('.');
        parts[1] = (parts[1][0] == 'a' ? 'b' : 'a') + parts[1][1..];

        Assert.Null(await tokens.ReadAsync(TestAuth.RequestWithBearer(string.Join('.', parts))));
    }

    [Fact]
    public async Task Token_ExpiresAfterItsLifetime()
    {
        var time = new FakeTimeProvider(new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero));
        var tokens = TestAuth.Tokens(time);
        var token = await tokens.IssueAsync(Ann);

        time.Advance(SessionTokens.Lifetime - TimeSpan.FromMinutes(1));
        Assert.NotNull(await tokens.ReadAsync(TestAuth.RequestWithBearer(token)));

        time.Advance(TimeSpan.FromMinutes(2));
        Assert.Null(await tokens.ReadAsync(TestAuth.RequestWithBearer(token)));
    }
}
