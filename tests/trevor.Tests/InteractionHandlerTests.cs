using Microsoft.Extensions.Logging.Abstractions;
using trevor.Core;
using trevor.Infrastructure.Discord.Models;

namespace trevor.Tests;

public class InteractionHandlerTests
{
    private sealed class FakeCommand(string name, bool deferred, Func<string> run) : ICommand
    {
        public string Name => name;
        public bool IsDeferred => deferred;
        public Task<DiscordResponseData> ExecuteAsync(DiscordInteraction interaction, CancellationToken ct = default) =>
            Task.FromResult(DiscordResponseData.FromText(run()));
    }

    private static InteractionHandler Handler(params ICommand[] commands) =>
        new(new CommandRegistry(commands), NullLogger<InteractionHandler>.Instance);

    [Fact]
    public async Task Ping_ReturnsPong()
    {
        var result = await Handler().HandleAsync(TestInteractions.Ping());

        Assert.Equal(ResponseType.Pong, result.Response.Type);
        Assert.False(result.EnqueueDeferred);
    }

    [Fact]
    public async Task SyncCommand_ReturnsMessage()
    {
        var result = await Handler(new FakeCommand("hello", false, () => "hi")).HandleAsync(TestInteractions.Command("hello"));

        Assert.Equal(ResponseType.ChannelMessage, result.Response.Type);
        Assert.Equal("hi", result.Response.Data?.Content);
        Assert.False(result.EnqueueDeferred);
    }

    [Fact]
    public async Task CommandNames_AreCaseInsensitive()
    {
        var result = await Handler(new FakeCommand("hello", false, () => "hi")).HandleAsync(TestInteractions.Command("HELLO"));

        Assert.Equal("hi", result.Response.Data?.Content);
    }

    [Fact]
    public async Task DeferredCommand_IsAcknowledgedAndEnqueued_NotExecuted()
    {
        var executed = false;
        var command = new FakeCommand("slow", true, () => { executed = true; return "done"; });

        var result = await Handler(command).HandleAsync(TestInteractions.Command("slow"));

        Assert.Equal(ResponseType.DeferredChannelMessage, result.Response.Type);
        Assert.True(result.EnqueueDeferred);
        Assert.False(executed);
    }

    [Fact]
    public async Task ExecuteDeferred_RunsCommand()
    {
        var text = await Handler(new FakeCommand("slow", true, () => "done")).ExecuteDeferredAsync(TestInteractions.Command("slow"));

        Assert.Equal("done", text.Content);
    }

    [Fact]
    public async Task UnknownCommand_ReturnsTrevorMessage_InsteadOfFailing()
    {
        var result = await Handler().HandleAsync(TestInteractions.Command("nope"));

        Assert.Equal(ResponseType.ChannelMessage, result.Response.Type);
        Assert.Contains("/nope", result.Response.Data?.Content);
    }

    [Fact]
    public async Task FailingCommand_ReturnsErrorMessage()
    {
        var command = new FakeCommand("boom", false, () => throw new InvalidOperationException());

        var result = await Handler(command).HandleAsync(TestInteractions.Command("boom"));

        Assert.Equal(InteractionHandler.ErrorMessage, result.Response.Data?.Content);
    }

    [Fact]
    public async Task FailingDeferredCommand_ReturnsErrorMessage()
    {
        var command = new FakeCommand("boom", true, () => throw new InvalidOperationException());

        var text = await Handler(command).ExecuteDeferredAsync(TestInteractions.Command("boom"));

        Assert.Equal(InteractionHandler.ErrorMessage, text.Content);
    }
}
