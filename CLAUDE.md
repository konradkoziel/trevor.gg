# CLAUDE.md — notes for working on trevor.gg

Discord bot "Trevor" (Trevor Philips persona) for a private friends' server, mostly League of Legends.
Azure Functions (.NET 8, isolated worker, ASP.NET Core integration) + Azure OpenAI + Azure Storage Queue.

## Talking to the owner

- The owner writes in Polish; answer in Polish. Code, identifiers and commit messages stay in English.
- User-facing bot strings are Polish and in Trevor's voice (sarcastic, crude is fine). Keep that tone in new messages.
- The `TimerFunction` message ("BBualdo to noob" every 5 min) is an intentional in-group joke, not a bug. Don't remove it unasked.

## Deploy flow — read before pushing

- **Every push to `main` deploys to production** (`.github/workflows/Trevor.yml`: build → `dotnet publish trevor.csproj` → OIDC login → `azure/functions-action`, ~1.5 min). Never push to `main` directly.
- Work on a branch, open a PR to `main`. `.github/workflows/build.yml` builds and runs the tests on every PR.
- **Local build in cloud sessions:** the dotnet-install script is blocked, but Ubuntu's own package works:
  `apt-get update && apt-get install -y dotnet-sdk-8.0` (~3 min). Then `dotnet build -c Release && dotnet test -c Release --no-build`. NuGet is reachable.
- Azure Functions Core Tools can't be installed (downloads from `pkgs.dev.azure.com`, blocked), so the app can't be run end-to-end locally. Check the generated bindings in `bin/Release/net8.0/functions.metadata` instead.
- After merge, check the `Deploy Azure Function` run via GitHub Actions; the owner tests on Discord (Claude has no Discord access).
- `main.tf` is empty — infrastructure is not in code. Azure resources (Function App `Trevor`, storage account, OpenAI deployment) were created by hand.

## Configuration (env vars / Azure app settings)

Bound to options classes in `Program.cs`; required ones are validated **at startup** (`ValidateOnStart`) — a missing value stops the host.

| Name | Options | Notes |
|---|---|---|
| `DISCORD_API_KEY` | `DiscordOptions.PublicKey` | Discord application **public key** (64 hex chars), despite the name. Required |
| `DISCORD_URL_OGÓLNY` | `DiscordOptions.GeneralWebhookUrl` | Webhook for the general channel (timer). Optional. Non-ASCII name — don't rename without changing the Azure setting |
| `DISCORD_SKIP_SIGNATURE=true` | `DiscordOptions.SkipSignatureValidation` | **Disables signature verification.** Local only, never in Azure |
| `OPENAI_URL`, `OPENAI_KEY`, `OPENAI_DEPLOYMENT` | `OpenAiOptions` | Azure OpenAI. Required |
| `AzureWebJobsStorage` | Functions host | Storage account: timer leases and the `deferred-commands` queue |

Template: `local.settings.example.json` → copy to `local.settings.json` (gitignored).

## Discord Interactions — gotchas learned the hard way

- Bot runs on **HTTP interactions only** (no Gateway/WebSocket). It can't read messages, react to events or join voice. Anything like that needs a separate always-on process.
- **PING (`type: 1`) is sent only when the Interactions Endpoint URL is saved** in the Developer Portal. A broken PING handler stays invisible in prod until the URL changes. Covered by `InteractionHandlerTests.Ping_ReturnsPong`.
- The endpoint URL is `/api/DiscordCommands` — that's the `[Function("DiscordCommands")]` name. **Don't rename the function.**
- Must answer within **3 s**. Slow commands (`IsDeferred => true`) reply `type: 5` and are put on the `deferred-commands` queue; `DeferredCommandFunction` runs them and POSTs the followup. The interaction token is valid for 15 min. Queue tuning is in `host.json` (2 s polling, 3 attempts, then `deferred-commands-poison`).
- Every outgoing message goes through `DiscordResponseData.FromText`: adds `allowed_mentions: { parse: [] }` (nobody can make the bot ping `@everyone`) and truncates to 2000 chars. Use it for any new message.
- Discord JSON is snake_case: `DiscordJson.Options` uses `SnakeCaseLower`, and models pin names with `[JsonPropertyName]`.
- Option values can be string/number/bool → `InteractionOption.Value` is a `JsonElement`; read options with `interaction.GetOption("name")`.
- Slash-command definitions (names/options) are registered with Discord outside this repo. A new command needs registering there too: `/ping`, `/teams gracze:`, `/ask question:`.
- Never log request bodies or Discord URLs — they contain interaction tokens / webhook secrets.

## Code layout & conventions

- `Functions/` — entry points only: `DiscordInteractionsFunction` (HTTP, signature check, returns `InteractionOutput` = HTTP result + optional queue message), `DeferredCommandFunction` (queue), `TimerFunction`.
- `Core/` — `ICommand`, `CommandRegistry` (all `ICommand`s from DI, by name), `InteractionHandler` (PING / sync / deferred / unknown / errors → Trevor-style message).
- `Features/<Name>/` — one folder per feature (vertical slices): `Ask` (+ `TrevorPrompts` persona), `Teams`, `Ping`.
- `Infrastructure/Discord/` — models, `DiscordJson`, `DiscordApiClient` (typed HttpClient), `DiscordSignatureVerifier`, `DiscordOptions`. `Infrastructure/OpenAi/` — options.
- **New command:** class in `Features/<Name>/` implementing `ICommand` → `AddSingleton<ICommand, XCommand>()` in `Program.cs` → register in Discord → tests in `tests/trevor.Tests`.
- Tests: xUnit in `tests/trevor.Tests` (excluded from the app build in `trevor.csproj`). Build interactions from Discord-shaped JSON with `TestInteractions`.
- Style: file-scoped namespaces, primary constructors, records, `ImplicitUsings` + `Nullable` on, zero build warnings. App source files are **UTF-8 with BOM, LF**.

## Roadmap

See `docs/ROADMAP.md` (Polish) — ranked feature ideas and the chosen patterns. Not done yet: Riot API features, `/lfg` buttons, results tracking, rank-balanced teams (Strategy), Table Storage repositories, IaC.
