using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Time.Testing;
using Web.Api.Features.Builds;
using Web.Api.Infrastructure.Http;
using Web.Api.Infrastructure.Sessions;

namespace Web.Api.Tests;

public class BuildFunctionsTests
{
    private static readonly SessionUser Ann = new("1", "Ann", "");
    private static readonly SessionUser Bob = new("2", "Bob", "");
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    private readonly FakeBuildStore _store = new();
    private readonly SessionTokens _tokens = TestAuth.Tokens();
    private readonly FakeTimeProvider _time = new(Now);

    private async Task<HttpRequest> Request(SessionUser? user, object? body = null)
    {
        var context = new DefaultHttpContext();
        if (user is not null)
            context.Request.Headers.Authorization = $"Bearer {await _tokens.IssueAsync(user)}";
        if (body is not null)
        {
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body as string ?? JsonSerializer.Serialize(body, JsonSerializerOptions.Web)));
            context.Request.ContentType = "application/json";
        }
        return context.Request;
    }

    private async Task<IActionResult> Create(SessionUser? user, object? body) =>
        await new CreateBuildFunction(_tokens, TestBuilds.Validator(), _store, _time).Run(await Request(user, body), CancellationToken.None);

    private StoredBuild Stored(string id, SessionUser owner) => new(id, owner.DiscordId, owner.Name, Now, TestBuilds.Valid());

    [Fact]
    public async Task Create_SavesTheBuild_UnderItsOwner()
    {
        var result = await Create(Ann, TestBuilds.Valid() with { Name = "  Yasuo  " });

        var created = Assert.IsType<BuildCreated>(Assert.IsType<ObjectResult>(result).Value);
        Assert.Equal(StatusCodes.Status201Created, ((ObjectResult)result).StatusCode);
        Assert.True(BuildIds.IsWellFormed(created.Id));
        var saved = _store.Builds[created.Id];
        Assert.Equal("1", saved.OwnerId);
        Assert.Equal("Ann", saved.OwnerName);
        Assert.Equal("Yasuo", saved.Build.Name);
        Assert.Equal(Now, saved.UpdatedAt);
    }

    [Fact]
    public async Task Create_NeedsLogin() => Assert.IsType<UnauthorizedResult>(await Create(null, TestBuilds.Valid()));

    [Fact]
    public async Task Create_InvalidBuild_Is400WithTheReason()
    {
        var result = await Create(Ann, TestBuilds.Valid() with { ChampionId = "Nobody" });

        Assert.Equal(BuildMessages.UnknownChampion, Assert.IsType<ApiError>(Assert.IsType<BadRequestObjectResult>(result).Value).Error);
        Assert.Empty(_store.Builds);
    }

    [Fact]
    public async Task Create_BrokenJson_Is400() => Assert.IsType<BadRequestResult>(await Create(Ann, "{ not json"));

    [Fact]
    public async Task Create_WithoutJsonContentType_Is400()
    {
        var req = await Request(Ann, TestBuilds.Valid());
        req.ContentType = "text/plain";

        Assert.IsType<BadRequestResult>(await new CreateBuildFunction(_tokens, TestBuilds.Validator(), _store, _time).Run(req, CancellationToken.None));
    }

    [Fact]
    public async Task Create_StopsAtTheLimit()
    {
        for (var i = 0; i < CreateBuildFunction.MaxBuildsPerUser; i++)
            _store.Builds[$"id-{i}"] = Stored($"id-{i}", Ann);

        var result = await Create(Ann, TestBuilds.Valid());

        Assert.Equal(BuildMessages.TooManyBuilds, Assert.IsType<ApiError>(Assert.IsType<BadRequestObjectResult>(result).Value).Error);
    }

    [Fact]
    public async Task Get_IsPublic_AndTellsTheOwnerItIsTheirs()
    {
        _store.Builds["AAAAAAAAAAA"] = Stored("AAAAAAAAAAA", Ann);
        var get = new GetBuildFunction(_tokens, _store);

        var anonymous = (BuildResponse)Assert.IsType<OkObjectResult>(await get.Run(await Request(null), "AAAAAAAAAAA", CancellationToken.None)).Value!;
        var owner = (BuildResponse)Assert.IsType<OkObjectResult>(await get.Run(await Request(Ann), "AAAAAAAAAAA", CancellationToken.None)).Value!;

        Assert.False(anonymous.IsMine);
        Assert.Equal("Ann", anonymous.OwnerName);
        Assert.True(owner.IsMine);
    }

    [Theory]
    [InlineData("BBBBBBBBBBB")] // well-formed, not there
    [InlineData("../../x")]
    public async Task Get_Unknown_Is404(string id) =>
        Assert.IsType<NotFoundResult>(await new GetBuildFunction(_tokens, _store).Run(await Request(null), id, CancellationToken.None));

    [Fact]
    public async Task Update_ByOwner_ReplacesTheBuild()
    {
        _store.Builds["AAAAAAAAAAA"] = Stored("AAAAAAAAAAA", Ann);
        _time.Advance(TimeSpan.FromHours(1));

        var result = await new UpdateBuildFunction(_tokens, TestBuilds.Validator(), _store, _time)
            .Run(await Request(Ann, TestBuilds.Valid() with { Name = "Nowa nazwa" }), "AAAAAAAAAAA", CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Nowa nazwa", _store.Builds["AAAAAAAAAAA"].Build.Name);
        Assert.Equal(Now.AddHours(1), _store.Builds["AAAAAAAAAAA"].UpdatedAt);
    }

    [Fact]
    public async Task Update_BySomeoneElse_Is403_AndChangesNothing()
    {
        _store.Builds["AAAAAAAAAAA"] = Stored("AAAAAAAAAAA", Ann);

        var result = await new UpdateBuildFunction(_tokens, TestBuilds.Validator(), _store, _time)
            .Run(await Request(Bob, TestBuilds.Valid() with { Name = "Moje teraz" }), "AAAAAAAAAAA", CancellationToken.None);

        Assert.Equal(StatusCodes.Status403Forbidden, Assert.IsType<ObjectResult>(result).StatusCode);
        Assert.Equal(TestBuilds.Valid().Name, _store.Builds["AAAAAAAAAAA"].Build.Name);
    }

    [Fact]
    public async Task Delete_ByOwnerOnly()
    {
        _store.Builds["AAAAAAAAAAA"] = Stored("AAAAAAAAAAA", Ann);
        var delete = new DeleteBuildFunction(_tokens, _store);

        Assert.Equal(StatusCodes.Status403Forbidden,
            Assert.IsType<ObjectResult>(await delete.Run(await Request(Bob), "AAAAAAAAAAA", CancellationToken.None)).StatusCode);
        Assert.IsType<NoContentResult>(await delete.Run(await Request(Ann), "AAAAAAAAAAA", CancellationToken.None));
        Assert.Empty(_store.Builds);
    }

    [Fact]
    public async Task MyBuilds_AreOnlyMine_NewestFirst()
    {
        _store.Builds["a"] = Stored("a", Ann) with { UpdatedAt = Now };
        _store.Builds["b"] = Stored("b", Ann) with { UpdatedAt = Now.AddDays(1) };
        _store.Builds["c"] = Stored("c", Bob);

        var result = await new MyBuildsFunction(_tokens, _store).Run(await Request(Ann), CancellationToken.None);

        var list = Assert.IsAssignableFrom<IEnumerable<BuildSummary>>(Assert.IsType<OkObjectResult>(result).Value);
        Assert.Equal(["b", "a"], list.Select(b => b.Id));
    }

    [Fact]
    public async Task MyBuilds_NeedsLogin() =>
        Assert.IsType<UnauthorizedResult>(await new MyBuildsFunction(_tokens, _store).Run(await Request(null), CancellationToken.None));
}
