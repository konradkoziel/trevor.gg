using Microsoft.AspNetCore.Mvc;
using Web.Api.Features.Account;
using Web.Api.Infrastructure.Sessions;

namespace Web.Api.Tests;

public class MeFunctionTests
{
    [Fact]
    public async Task WithoutToken_Is401()
    {
        var result = await new MeFunction(TestAuth.Tokens()).Run(TestAuth.RequestWithBearer(null), CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task WithToken_ReturnsTheUser()
    {
        var tokens = TestAuth.Tokens();
        var ann = new SessionUser("1", "Ann", "https://cdn.test/a.png");
        var token = await tokens.IssueAsync(ann);

        var result = await new MeFunction(tokens).Run(TestAuth.RequestWithBearer(token), CancellationToken.None);

        Assert.Equal(ann, Assert.IsType<OkObjectResult>(result).Value);
    }
}
