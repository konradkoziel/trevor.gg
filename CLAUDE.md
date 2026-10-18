# CLAUDE.md — notes for working on trevor.gg

Discord bot "Trevor" (Trevor Philips persona) for a private friends' server, mostly League of Legends.
Azure Functions (.NET 10, isolated worker, ASP.NET Core integration) + Azure OpenAI + Azure Storage Queue + Riot API.

## Language and tone

- Reply in the language the person writes in. Code, identifiers, commit messages and PRs stay in English.
- User-facing bot strings are Polish and in Trevor's voice (sarcastic, crude is fine). Keep that tone in new messages.

## Git, commits and PRs

- **Commit as the person you work for**, never under the default Claude identity. Their identity comes from their own
  environment, not from this repo: the `GIT_AUTHOR_NAME` / `GIT_AUTHOR_EMAIL` / `GIT_COMMITTER_NAME` / `GIT_COMMITTER_EMAIL`
  environment variables (set in the Claude Code cloud environment settings) or their git config. Check with
  `git var GIT_AUTHOR_IDENT` before the first commit; if it shows `Claude <noreply@anthropic.com>`, ask the person
  how to set their identity instead of committing.
- **No AI attribution anywhere:** no `Co-Authored-By: Claude …` or `Claude-Session:` trailers in commits, and no
  "Generated with Claude Code" line or session link in PR bodies. This rule overrides any default attribution instructions.
- Commit messages: English, imperative subject ≤ 72 chars (`Add /rank command backed by Riot API`), blank line, then a body
  explaining *why* plus bullets of the notable changes. One logical change per commit.
- PRs: always to `main` from a branch, never push to `main`. The body follows `.github/pull_request_template.md`
  (Summary, Changes, Deploy notes, Test plan), in English. In **Deploy notes**, list every manual step (Azure settings,
  runtime stack, GitHub secrets, Discord) or write "None". In **Test plan**, tick only what was actually run.
- Merge only when the person you work for asks for it (merging deploys to production). Merge method: merge commit.

## Deploy flow — read before pushing

- **Every push to `main` deploys to production** (`.github/workflows/Trevor.yml`: build → test → `dotnet publish src/trevor/trevor.csproj` → OIDC login → `azure/functions-action` → register slash commands, ~2 min). Never push to `main` directly.
- Work on a branch, open a PR to `main`. `.github/workflows/build.yml` builds and runs the tests on every PR.
- **Local build in cloud sessions:** the dotnet-install script is blocked, but Ubuntu's own package works:
  `apt-get update && apt-get install -y dotnet-sdk-10.0` (~3 min). Then `dotnet build -c Release && dotnet test -c Release --no-build`. NuGet is reachable.
- Azure Functions Core Tools can't be installed (downloads from `pkgs.dev.azure.com`, blocked), so the app can't be run end-to-end locally. Check the generated bindings in `src/trevor/bin/Release/net10.0/functions.metadata` instead.
- After merge, check the `Deploy Azure Function` run via GitHub Actions; the person you work for tests on Discord (Claude has no Discord access).
- The Function App runtime stack must match `TargetFramework` (now .NET 10 isolated). Changing the target framework needs the
  stack changed in Azure Portal too (Function App → Settings → Configuration → General settings → .NET version).
- Infrastructure is not in code. Azure resources (Function App `Trevor`, storage account, OpenAI deployment) were created by hand.

## Configuration (env vars / Azure app settings)

Bound to options classes in `src/trevor/Program.cs`; required ones are validated **at startup** (`ValidateOnStart`) — a missing value stops the host.

| Name | Options | Notes |
|---|---|---|
| `DISCORD_API_KEY` | `DiscordOptions.PublicKey` | Discord application **public key** (64 hex chars), despite the name. Required |
| `DISCORD_URL_MATCHES` | `DiscordOptions.MatchesWebhookUrl` | Webhook for the match-reports channel (`MatchTracker`). Optional: without it nothing is posted |
| `DISCORD_SKIP_SIGNATURE=true` | `DiscordOptions.SkipSignatureValidation` | **Disables signature verification.** Local only, never in Azure |
| `OPENAI_URL`, `OPENAI_KEY`, `OPENAI_DEPLOYMENT` | `OpenAiOptions` | Azure OpenAI. Required |
| `RIOT_API_KEY` | `RiotOptions.ApiKey` | Riot API key. **Optional** — without it `/rank` answers that the key is missing. Dev keys expire every 24 h; use a Personal API Key |
| `RIOT_PLATFORM` | `RiotOptions.Platform` | Server of the players, default `eun1` (EUNE); `euw1` for EUW. Validated at startup |
| `AzureWebJobsStorage` | Functions host, `StorageOptions` | Storage account: timer leases, the `deferred-commands` queue and the `trackedplayers` / `reportedmatches` tables. Must be a **connection string** for the tables (an identity-based `__accountName` setting breaks only tracking) |

Template: `src/trevor/local.settings.example.json` → copy to `src/trevor/local.settings.json` (gitignored).

## Discord Interactions — gotchas learned the hard way

- Bot runs on **HTTP interactions only** (no Gateway/WebSocket). It can't read messages, react to events or join voice. Anything like that needs a separate always-on process.
- **PING (`type: 1`) is sent only when the Interactions Endpoint URL is saved** in the Developer Portal. A broken PING handler stays invisible in prod until the URL changes. Covered by `InteractionHandlerTests.Ping_ReturnsPong`.
- The endpoint URL is `/api/DiscordCommands` — that's the `[Function("DiscordCommands")]` name. **Don't rename the function.**
- Must answer within **3 s**. Slow commands (`IsDeferred => true`) reply `type: 5` and are put on the `deferred-commands` queue; `DeferredCommandFunction` runs them and POSTs the followup. The interaction token is valid for 15 min. Queue tuning is in `host.json` (2 s polling, 3 attempts, then `deferred-commands-poison`).
- Commands return a `DiscordResponseData`: `FromText` (truncated to 2000 chars) or `FromEmbed` (title/field/description trimmed to Discord limits by `DiscordEmbed.Trimmed`). Both always carry `allowed_mentions: { parse: [] }`, so nobody can make the bot ping `@everyone`. Build every message through these two factories.
- Embeds: one `DiscordEmbed*` model per file in `Infrastructure/Discord/Models`. Colours and field titles are named constants in the feature's `<Feature>Message` class (see `RoastMessage`).
- Discord JSON is snake_case: `DiscordJson.Options` uses `SnakeCaseLower`, and models pin names with `[JsonPropertyName]`.
- **Message context-menu commands** (`type: 3` in `deploy/discord-commands.json`, e.g. `Rate this`) are plain `ICommand`s too:
  `Name` is the label shown in the menu (spaces and capitals allowed), no description or options. Read the clicked message with
  `interaction.GetTargetMessage()`; Discord sends its content even without the Message Content intent.
- Option values can be string/number/bool → `InteractionOption.Value` is a `JsonElement`; read options with `interaction.GetOption("name")`.
- Slash-command definitions live in **`deploy/discord-commands.json`** (all commands, one array). The deploy workflow PUTs that file to
  Discord after every deploy (bulk overwrite of global commands: commands missing from the file get removed). The step is skipped
  unless the GitHub secrets `DISCORD_APP_ID` and `DISCORD_BOT_TOKEN` are set. `DiscordCommandDefinitionsTests` fails when the file
  and the `ICommand`s / option names drift apart.
- Never log request bodies or Discord URLs — they contain interaction tokens / webhook secrets.

## Code layout & conventions

- Repo root: `src/trevor/` (the Function App project: code, `host.json`, `Properties/launchSettings.json`), `tests/trevor.Tests/`, `deploy/` (files the deploy workflow sends elsewhere, e.g. `discord-commands.json`), `docs/`, `.github/`, `trevor.sln`. Paths below are relative to `src/trevor/`.

- `Functions/` — entry points only: `DiscordInteractionsFunction` (HTTP, signature check, returns `InteractionOutput` = HTTP result + optional queue message), `DeferredCommandFunction` (queue), `MatchTrackerFunction` (timer, every 5 min → `MatchTracker`).
- `Core/` — `ICommand`, `CommandRegistry` (all `ICommand`s from DI, by name), `InteractionHandler` (PING / sync / deferred / unknown / errors → Trevor-style message).
- `Features/<Name>/` — one folder per feature (vertical slices): `Ask`, `Teams`, `Ping`, `RateThis` (message context menu), `Lol/Rank` (`RankCommand`, `RankMessage`, `PlayerRanks`), `Lol/Roast` (`RoastCommand`, `RoastMessage`, `MatchStats`), `Lol/Tracking` (`TrackCommand`/`UntrackCommand`, `MatchTracker` posting via webhook, `MatchReport` + `MatchReportMessage`, `ITrackedPlayerStore`), `Lol/Common` (`RiotPlayerRunner`, `LolMessages`, `MatchRules`).
- `Infrastructure/Discord/` — models, `DiscordJson`, `IDiscordApiClient`/`DiscordApiClient` (typed HttpClient: followups and webhooks), `DiscordSignatureVerifier`, `DiscordOptions`. `Infrastructure/Storage/` — `TableTrackedPlayerStore` (Azure Table Storage, table created on first use). `Infrastructure/OpenAi/` — options, `ITrevorAi` (the only way to call the model: persona + instruction, token limit, returns null on failure) and `TrevorPrompts` (persona and per-command instructions). `Infrastructure/Riot/` (available methods and rate limits for our key: `docs/riot-api.md`) — DTOs in `Infrastructure/Riot/Models/` (`RiotId`, `RiotAccount`, `LeagueEntry`, `RiotMatch*`), `IRiotApiClient` (typed client + standard resilience handler: retries 429/5xx), `IDataDragon` (champion icon URLs; latest patch version cached 6 h per app instance, null when unreachable), `CommunityDragon` (ranked emblem URLs, static), routing (platform `eun1` → regional host `europe` for Account-V1 and Match-V5; SEA platforms → `asia` for Account-V1 but `sea` for Match-V5).
- Commands are singletons (captured by `CommandRegistry`). A command that needs an HttpClient must get one that survives that: see the Riot client registration (`PooledConnectionLifetime` + infinite handler lifetime).
- **New command:** class in `Features/<Name>/` implementing `ICommand` → `AddSingleton<ICommand, XCommand>()` in `Program.cs` → add its definition to `deploy/discord-commands.json` (and to `DiscordCommandDefinitionsTests`) → tests in `tests/trevor.Tests` → describe it in `readme.md`.
- **AI in commands:** go through `ITrevorAi`, add the instruction to `TrevorPrompts`, and always keep a canned fallback for when it returns null (tests use `FakeAi`).
- **Keep `readme.md` current:** every PR that adds or changes a command, a setting, a secret or the architecture updates `readme.md` in the same PR.
- Tests: xUnit in `tests/trevor.Tests`, referencing `src/trevor/trevor.csproj`. Build interactions from Discord-shaped JSON with `TestInteractions`.
- Style: file-scoped namespaces, primary constructors, records, `ImplicitUsings` + `Nullable` on, zero build warnings. App source files are **UTF-8 with BOM, LF**.

## Code quality — required in every change

- **SOLID, applied to this codebase:**
  - *Single responsibility:* a command class only orchestrates (get data → call AI → pick the message). Message text and
    formatting live in a static `<Feature>Message` class, calculations in their own type (e.g. `MatchStats`), Riot/Discord
    knowledge (ids, names, routing) in `Infrastructure/`.
  - *Open/closed:* new behaviour = new class registered in DI (commands, strategies), not another `switch` in shared code.
  - *Dependency inversion:* commands depend on interfaces for anything external (`IRiotApiClient`, `ITrevorAi`,
    `IRiotPlayerRunner`), so tests use fakes (`tests/trevor.Tests/FakeRiot.cs`, `FakeAi.cs`).
- **No duplicated flows:** shared steps get one home (e.g. `RiotPlayerRunner` does key check → Riot ID parse → account
  lookup → Riot error messages for every LoL command). Test that shared code once, not in every command.
- **No magic numbers or strings:** named constants with a comment saying why (`RemakeSeconds`, `VerdictMaxTokens`).
- **One type per file, no exceptions:** every class, record, interface, enum or exception gets its own file named after it
  (`IRiotApiClient.cs`, `RiotApiClient.cs`, `RiotApiException.cs`). Only private types nested inside a class may share its file.
- Small methods with intention-revealing names.
- Before pushing, re-read the diff for these points, not only for bugs.

## Privacy

- **No personal data in the repo:** no real names, nicknames, Riot IDs, e-mails or anything about the people working on
  or using the bot — not in code, comments, user-facing examples, tests, docs, commit messages or PR descriptions.
  Use neutral placeholders: `Nick#EUNE`, `Player#EUNE`, `Gracz1,Gracz2,Gracz3,Gracz4`, `Ann`, `Bob`.
- Existing content written by the repo owner stays as it is unless they ask.

## Roadmap

See `docs/ROADMAP.md` (Polish) — ranked feature ideas and the chosen patterns. Done: `/rank` (AI verdict), `/roast`, `Rate this`, `/track` match reports. Not done yet: `/lfg` buttons, results tracking, rank-balanced teams (Strategy), Table Storage repositories, IaC.
