# CLAUDE.md — notes for working on trevor.gg

Discord bot "Trevor" (Trevor Philips persona) for a private friends' server, mostly League of Legends.
Azure Functions (.NET 8, isolated worker) + Azure OpenAI. ~1k lines, no tests yet.

## Talking to the owner

- The owner writes in Polish; answer in Polish. Code, identifiers and commit messages stay in English.
- User-facing bot strings are Polish and in Trevor's voice (sarcastic, crude is fine). Keep that tone in new messages.
- The `TimerFunction` message ("BBualdo to noob" every 5 min) is an intentional in-group joke, not a bug. Don't remove it unasked.

## Deploy flow — read before pushing

- **Every push to `main` deploys to production** (`.github/workflows/Trevor.yml`: build → publish → OIDC login → `azure/functions-action`, ~1.5 min). Never push to `main` directly.
- Work on a branch, open a PR to `main`. `.github/workflows/build.yml` builds on every PR — that is the only compile check.
- Cloud sessions can't install the .NET SDK (proxy blocks `builds.dotnet.microsoft.com`), so code can't be built locally there. Rely on the PR `Build` check; read the code twice before pushing.
- After merge, check the `Deploy Azure Function` run via GitHub Actions; the owner tests on Discord (Claude has no Discord access).
- `main.tf` is empty — infrastructure is not in code. Azure resources (Function App `Trevor`, storage account, OpenAI deployment) were created by hand.

## Configuration (env vars / Azure app settings)

| Name | Used by | Notes |
|---|---|---|
| `DISCORD_API_KEY` | `Common/Authentication.cs` | Discord application **public key** (hex), despite the name |
| `OPENAI_URL`, `OPENAI_KEY`, `OPENAI_DEPLOYMENT` | `Program.cs` | Azure OpenAI; app fails at first `ChatClient` resolve if missing |
| `DISCORD_URL_OGÓLNY` | `Functions/TimerFunction.cs` | Webhook URL for the general channel. Non-ASCII name — don't rename without changing the Azure setting too |
| `DOTNET_ENVIRONMENT=Development` | `CommandFunction.cs` | **Disables signature verification.** Local only |
| `AzureWebJobsStorage` | Functions host | Storage account, needed by the timer trigger |

`local.settings.json` is gitignored; there's no example file.

## Discord Interactions — gotchas learned the hard way

- Bot runs on **HTTP interactions only** (no Gateway/WebSocket). It can't read messages, react to events or join voice. Anything like that needs a separate always-on process.
- **PING (`type: 1`) is sent only when the Interactions Endpoint URL is saved** in the Developer Portal. A broken PING handler stays invisible in prod until the URL changes. It's handled in `CommandFunction` before the command-name check — keep it there.
- Must answer within **3 s**. Slow work (OpenAI) → reply `type: 5` (deferred), then POST a followup to `https://discord.com/api/v10/webhooks/{application_id}/{token}`. The token is valid for 15 min.
- Deferred work currently runs in fire-and-forget `Task.Run` (`CommandHandler`). It can be lost if the host recycles. The planned fix is a Storage Queue (see `docs/ROADMAP.md`).
- Every outgoing message carries `allowed_mentions: { parse: [] }` (`DiscordResponseData`) so nobody can make the bot ping `@everyone`. Keep it on any new message type.
- Discord JSON is snake_case. The serializer uses `CamelCase`, so multi-word fields need `[JsonPropertyName("snake_case")]`.
- Message content limit: 2000 chars.
- Option values can be string/number/bool, but `InteractionOption.Value` is typed `string`. A non-string option will break deserialization; change it to `JsonElement` before adding one.
- Slash-command definitions (names/options) are registered with Discord outside this repo. A new command needs registering there too: `/ping`, `/teams gracze:`, `/ask question:`.

## Code layout & conventions

- `Functions/` — Azure Function entry points (HTTP `DiscordCommands`, timer).
- `Commands/` — one `ICommand` per slash command; `Commands/Core/CommandFactory` maps name → command (switch); `CommandHandler` decides sync (`type 4`) vs deferred (`type 5`) by `IsDefferedType` (sic).
- `Model/` — Discord DTOs as records. Folder `Model/Response.cs/` and namespace `trevor.Model.Response.cs` are an accident, but other files depend on them.
- Style: file-scoped or block namespaces (mixed), primary constructors, records, `ImplicitUsings` + `Nullable` on. Source files are **UTF-8 with BOM, LF**. Keep the BOM when rewriting files.
- `JsonSerializerOptions` are duplicated in `CommandFunction` and `TimerFunction`.

## Known open items from the code review (not fixed yet)

- `Task.Run` for deferred commands → queue (biggest reliability risk).
- `InteractionOption.Value` as `string` (see above).
- Unknown command → `NotSupportedException` → HTTP 400 → "This interaction failed" for the user.
- Full request bodies (including interaction tokens) are logged to Application Insights.
- `IAuthentication` registered twice in `Program.cs`. Unused `Discord.Net.*` packages. Unused named HttpClient `"discord"`.
- No tests, no IaC, no `local.settings.example.json`.
