# CLAUDE.md — notes for working on trevor.gg

Discord bot "Trevor" (Trevor Philips persona) for a private friends' server, mostly League of Legends.
Azure Functions (.NET 10, isolated worker, ASP.NET Core integration) + Azure OpenAI + Azure Storage Queue + Riot API.
The same repo holds the **website** (builds and stats): React + Vite + TypeScript in `src/web`, its API in `src/Web.Api`
(a separate Function App), Azure resources in `infra/web.bicep`.

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
- PRs: always to `main` from a branch, never push to `main`. The body follows `.github/pull_request_template.md`, in English,
  and is **short**: a Summary of 1–3 brief bullets (what changes and why), no file-by-file list and **no test plan**.
  Add **Deploy notes** only when something must be done by hand (GitHub secrets, Azure, Discord); otherwise leave the section out.
- Merge only when the person you work for asks for it (merging deploys to production). Merge method: merge commit.

## Deploy flow — read before pushing

- `bot.yml` ignores `**.md`, `docs/**` and website-only paths, so a docs or site change doesn't redeploy the bot.
- **Every push to `main` deploys to production** (`.github/workflows/bot.yml`, "Deploy bot": build → test → `dotnet publish src/trevor/trevor.csproj` → OIDC login → `azure/functions-action` → register slash commands, ~2 min). Never push to `main` directly.
- **The website deploys separately** (`.github/workflows/web.yml`, only when `src/web`, `src/Web.Api`, `src/Lol.Shared`, `infra` or the
  workflow change): tests → `az deployment group create` of `infra/web.bicep` into resource group `trevor-web` (Static Web App
  `trevor-web` on the Free plan, Flex Consumption Function App `trevor-web-api-*`, its own storage and Application Insights) →
  API package → `npm run build` with `VITE_API_URL` → Static Web App upload. `bot.yml` ignores website-only paths.
  The deploy identity (`AZURE_CLIENT_ID`) has Contributor on `trevor-web` only. Static Web Apps don't exist in Poland Central,
  hence `staticSiteLocation = westeurope`. Managed SWA functions lag behind .NET releases, which is why the API is its own Function App.
- Work on a branch, open a PR to `main`. `.github/workflows/build.yml` ("Build and test") builds and runs the tests on every PR.
- **Local build in cloud sessions:** the dotnet-install script is blocked, but Ubuntu's own package works:
  `apt-get update && apt-get install -y dotnet-sdk-10.0` (~3 min). Then `dotnet build -c Release && dotnet test -c Release --no-build`. NuGet is reachable.
  Website: `cd src/web && npm ci && npm run lint && npm test && npm run build` (npm is reachable). Bicep CLI: download
  `https://github.com/Azure/bicep/releases/latest/download/bicep-linux-x64`, then `bicep build infra/web.bicep` / `bicep lint`.
  Data Dragon (`ddragon.leagueoflegends.com`) is blocked in cloud sessions: screenshot the site with a mocked `/api`.
- Azure Functions Core Tools can't be installed (downloads from `pkgs.dev.azure.com`, blocked), so the app can't be run end-to-end locally. Check the generated bindings in `src/trevor/bin/Release/net10.0/functions.metadata` instead.
- After merge, check the `Deploy bot` run (and `Deploy website` when the site changed) via GitHub Actions; the person you work for tests on Discord (Claude has no Discord access).
- The Function App runtime stack must match `TargetFramework` (now .NET 10 isolated). Changing the target framework needs the
  stack changed in Azure Portal too (Function App → Settings → Configuration → General settings → .NET version).
- The bot's infrastructure is not in code: its Azure resources (Function App `Trevor`, storage account, OpenAI deployment) were
  created by hand, but its app settings are not: `bot.yml` writes them from GitHub secrets of the same names before each deploy
  (empty secrets are skipped, `AzureWebJobsStorage` is left alone). The website's resources are in `infra/web.bicep`; change them there, never in the Portal.

## Configuration (env vars / Azure app settings)

**One name per setting:** the GitHub secret, the bot's app setting and the API's app setting share it (`docs/setup.md` lists them).
Bound to options classes in `src/trevor/Program.cs` (the Riot ones in `AddRiot()`, `src/Lol.Shared`); required ones are validated **at startup** (`ValidateOnStart`) — a missing value stops the host.

| Name | Options | Notes |
|---|---|---|
| `DISCORD_PUBLIC_KEY` | `DiscordOptions.PublicKey` | Discord application **public key** (64 hex chars). Required (`bot.yml` stops before deploying without it) |
| `DISCORD_MATCHES_WEBHOOK` | `DiscordOptions.MatchesWebhookUrl` | Webhook for the match-reports channel (`MatchTracker`). Optional: without it nothing is posted |
| `WEB_API_URL`, `TREVOR_WEB_KEY`, `WEB_SITE_URL` | `WebApiOptions` | The website API for `/build` and nick-less `/rank` / `/roast`: its URL and the site URL (`bot.yml` reads both from the website's last deployment) and the key shared with the API. Optional. Only `bot.yml` writes the bot's settings; `web.yml` never touches the bot |
| `DISCORD_SKIP_SIGNATURE=true` | `DiscordOptions.SkipSignatureValidation` | **Disables signature verification.** Local only, never in Azure |
| `OPENAI_URL`, `OPENAI_KEY`, `OPENAI_DEPLOYMENT` | `OpenAiOptions` | Azure OpenAI. Required |
| `RIOT_API_KEY` | `RiotOptions.ApiKey` | Riot API key. **Optional** — without it `/rank` answers that the key is missing. Dev keys expire every 24 h; use a Personal API Key |
| `RIOT_PLATFORM` | `RiotOptions.Platform` | Server of the players, default `eun1` (EUNE); `euw1` for EUW. Validated at startup |
| `AzureWebJobsStorage` | Functions host, `StorageOptions` | Storage account: timer leases, the `deferred-commands` queue and the `trackedplayers` / `reportedmatches` tables. Must be a **connection string** for the tables (an identity-based `__accountName` setting breaks only tracking) |

Template: `src/trevor/local.settings.example.json` → copy to `src/trevor/local.settings.json` (gitignored).

Website API (`src/Web.Api`, template `src/Web.Api/local.settings.example.json`): all set by `infra/web.bicep`, never by hand.
`DISCORD_APP_ID` / `DISCORD_CLIENT_SECRET` / `TREVOR_WEB_KEY` (from the GitHub secrets of the same names),
`DISCORD_REDIRECT_URI` (`https://<api>.azurewebsites.net/api/auth/discord/callback`, must also be in the Developer Portal under
OAuth2 → Redirects), `SITE_URL`, `RIOT_API_KEY` (from the GitHub secret of the same name; one Riot key shared by both apps, so their
rate limits add up). All optional: without them only login / Riot account linking are off (503).

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

## Website login — how it works

- Authorization code flow with Discord, scope `identify` (+ `guilds.members.read` when `DISCORD_GUILD_ID` is set).
  `GET /api/auth/discord/login` sets a random `state` in an HttpOnly, Secure, SameSite=Lax cookie (path `/api/auth/discord`,
  value `<state>.s|c` = silent or consent attempt) and redirects to Discord with `prompt=none` first. The callback checks the state
  (CSRF); an `error` after the silent attempt restarts once with the consent screen, after the consent screen it is a cancel.
  Then it exchanges the code, reads `/users/@me` and, guild-only, `/users/@me/guilds/{id}/member` (404 = not a member →
  `#login-error=not-member`; the member's server nick and avatar win), and redirects to `SITE_URL/#session=<jwt>` (or `#login-error`).
  `DISCORD_GUILD_ID` comes from the GitHub **variable** of the same name (not a secret, but kept out of the code).
- The site (`src/web/src/auth/session.ts`) takes the token from the fragment, removes it from the address bar and keeps it in
  `localStorage`; `api/client.ts` sends it as `Authorization: Bearer`. A 401 clears it. Cookies can't carry the session because
  the site (`*.azurestaticapps.net`) and the API (`*.azurewebsites.net`) are different sites.
- Tokens: HS256 JWT, 7 days (`SessionTokens`). The signing key is generated on first use into the API's own storage
  (container `sessions`, blob `signing-key`); deleting it logs everybody out. Read the user in a function with
  `ISessionTokens.ReadAsync(req)`; null → 401.
- Never log the code, the tokens or Discord's token response.

## Code layout & conventions

- Repo root: `src/trevor/` (the Function App project: code, `host.json`, `Properties/launchSettings.json`), `src/Lol.Shared/`
  (class library with the League of Legends code every app in the repo shares: `Riot/`), `tests/trevor.Tests/`, `tests/Lol.Shared.Tests/`, `deploy/` (files the deploy workflow sends elsewhere, e.g. `discord-commands.json`), `src/Web.Api/` + `tests/Web.Api.Tests/` (website API: `Features/<Name>/` with one HTTP function each, e.g. `Champions` → `GET /api/champions`,
  `Champions` also `GET /api/champions/{id}/abilities` (only ids from the champion list), `Builds` → `GET /api/build-data` (items, rune trees,
  summoner spells), `Auth` → Discord login, `Account` → `GET /api/me`; builds CRUD: `POST /api/builds`, `GET|PUT|DELETE /api/builds/{id}` (GET public,
  PUT/DELETE owner only), `GET /api/me/builds`. `BuildValidator` re-checks every build against the patch and the game's rules
  (`SkillOrderRules` mirrors `src/web/src/features/builds/skillOrder.ts` — change both together). Builds are stored by
  `Infrastructure/Storage/TableBuildStore` in table `builds` of the API's storage (PartitionKey = build id, the build as JSON);
  `Share` → `GET /api/share/{id}` (the link "Kopiuj link" copies: an HTML page with Open Graph tags for Discord's crawler,
  which runs no JavaScript, and a meta refresh to `/b/{id}`; names via `BuildCards`); `Home` → `GET /api/rotation` (public, `FreeRotation` caches 1 h), `GET /api/leaderboard` (logged in; `Leaderboard` sums
  Champion-Mastery-V4 points per linked player plus the Solo/Duo rank, caches 3 h, skips players Riot fails on), `GET /api/recent-builds` (logged in;
  not `/api/builds/recent`, which would read as an id); `Bot` → `GET /api/bot/players/{discordId}` (linked Riot ID) and `.../builds?champion=` (newest-first `BuildCard`s with names, champion
  matched by name or id ignoring case/accents/punctuation), only with header `X-Trevor-Key` = `TREVOR_WEB_KEY`;
  `RiotAccount` → link a League account: `POST /api/me/riot` (Riot ID, no ownership check: login is guild-only and an account
  linked by someone else is a 409), `GET|DELETE /api/me/riot`; table `riotlinks` (PartitionKey = Discord id, row `link`); Riot failures become 503 with a message through `RiotCalls`; `Stats` → `GET /api/me/stats`:
  `MatchHistory` (singleton: last `MATCH_HISTORY_GAMES` match ids, default 20, from the GitHub variable of that name; only unknown matches fetched, older stored ones deleted, at most one Riot refresh per player per 2 min)
  over table `playedmatches` (PartitionKey = PUUID, RowKey = match id), numbers from `StatsCalculator` (Summoner's Rift queues,
  no remakes) plus the ranks, put together by `StatsReport` (also used by the profiles); `Matches` → `GET /api/matches/{matchId}` (logged in; both teams of one game with items, spells and runes, `MatchDetailSource` keeps
  up to 300 matches in memory for 6 h, a result without Data Dragon icons is not cached); `Players` → `GET /api/players/{name}/{tag}` (logged in; only linked players, Riot ID ignoring case: `PlayerProfiles` adds icon, level, top 5 masteries and the player's builds); `Ranks/RankedStandings` (League-V4 entries → Solo/Duo then Flex, with label and emblem, for every page); `Infrastructure/Http/ApiResults.StaticData` (200 cacheable 1 h or 503); `Infrastructure/Discord` OAuth client, `Infrastructure/Sessions` tokens),
  `src/web/` (React site: `src/api/` typed fetch helpers + `useApi` (pass a module-level loader), `src/features/<name>/` components;
  rules live in pure functions with vitest tests, e.g. `features/builds/skillOrder.ts`, `runes.ts`, `build.ts`; `src/pages/` one
  component per route of `react-router` (`/` home, `/buildy`, `/nowy/:championId`, `/b/:id`, `/konto`, `/statystyki`, `/gracz/:name/:tag`); daily picks
  (quote, champion) through `features/home/daily.ts` so everyone sees the same all day; styles in one `src/index.css` with colour
  and font tokens on `:root` (Cinzel for headings only, it has no lowercase; Inter for text; both self-hosted via `@fontsource`),
  `button gold` for the site's own actions and `button discord` only for Discord login; `features/layout/SiteFooter` carries Riot's
  required legal boilerplate, keep it; `CurrentUserProvider` shares the logged-in user),
  `infra/` (Bicep of the website), `docs/`, `.github/`, `trevor.sln`. Paths below are relative to `src/trevor/`, except `Riot/` (in `src/Lol.Shared/`).

- `Functions/` — entry points only: `DiscordInteractionsFunction` (HTTP, signature check, returns `InteractionOutput` = HTTP result + optional queue message), `DeferredCommandFunction` (queue), `MatchTrackerFunction` (timer, every 5 min → `MatchTracker`).
- `Core/` — `ICommand`, `CommandRegistry` (all `ICommand`s from DI, by name), `InteractionHandler` (PING / sync / deferred / unknown / errors → Trevor-style message).
- `Features/<Name>/` — one folder per feature (vertical slices): `Ask`, `Teams`, `Ping`, `RateThis` (message context menu), `Lol/Rank` (`RankCommand`, `RankMessage`, `PlayerRanks`), `Lol/Roast` (`RoastCommand`, `RoastMessage`, `MatchStats`), `Lol/Tracking` (`TrackCommand`/`UntrackCommand`, `MatchTracker` posting via webhook (no AI), `MatchReport` (MVP and report candidate = best / worst KDA of the tracked players' team, tracked players included) + `MatchReportMessage`, `ITrackedPlayerStore`), `Lol/Builds` (`BuildCommand` + `BuildMessage`: the caller's newest website build for a champion), `Lol/Common` (`RiotPlayerRunner`: nick option, or without it the account linked on the website, looked up again by Riot ID because PUUIDs differ per API key; `LolMessages`).
- `Infrastructure/Web/` — `IWebApiClient`/`WebApiClient` (typed HttpClient for the website's `/api/bot/...`, key in `X-Trevor-Key`; every failure is null, so a broken site only takes away the website features). The caller's Discord id: `interaction.GetUserId()` (`member.user` on a server, `user` in a DM).
- `Infrastructure/Discord/` — models, `DiscordJson`, `IDiscordApiClient`/`DiscordApiClient` (typed HttpClient: followups and webhooks), `DiscordSignatureVerifier`, `DiscordOptions`. `Infrastructure/Storage/` — `TableTrackedPlayerStore` (Azure Table Storage, table created on first use). `Infrastructure/OpenAi/` — options, `ITrevorAi` (the only way to call the model: persona + instruction, token limit, returns null on failure) and `TrevorPrompts` (persona and per-command instructions).
- `src/Lol.Shared/Riot/` (namespace `Lol.Shared.Riot`; available methods and rate limits for our key: `docs/riot-api.md`) — `AddRiot()` registers options and clients for any app, DTOs in `Riot/Models/` (`RiotId`, `RiotAccount`, `LeagueEntry`, `RiotMatch*`), `IRiotApiClient` (typed client + standard resilience handler: retries 429/5xx), `DataDragonCdn` (singleton: latest version cached 6 h, every file cached per patch, last good value when a download fails), `IDataDragon` (champion icon URLs and the champion list in Polish), `IDataDragonCatalog` (Summoner's Rift items, rune trees, summoner spells, champion abilities; HTML descriptions made plain by `DataDragonText`); all null when unreachable, `CommunityDragon` (ranked emblem URLs, static), `RankNames` (tier labels: `Gold II`, `Master`), `MatchRules` (remakes, shared by the bot and the site), routing (platform `eun1` → regional host `europe` for Account-V1 and Match-V5; SEA platforms → `asia` for Account-V1 but `sea` for Match-V5).
- Commands are singletons (captured by `CommandRegistry`). A command that needs an HttpClient must get one that survives that: see `RiotServiceCollectionExtensions.AddRiot` (`PooledConnectionLifetime` + infinite handler lifetime).
- **New command:** class in `Features/<Name>/` implementing `ICommand` → `AddSingleton<ICommand, XCommand>()` in `Program.cs` → add its definition to `deploy/discord-commands.json` (and to `DiscordCommandDefinitionsTests`) → tests in `tests/trevor.Tests` → describe it in `readme.md`.
- **AI in commands:** go through `ITrevorAi`, add the instruction to `TrevorPrompts`, and always keep a canned fallback for when it returns null (tests use `FakeAi`).
- **Keep `readme.md` current:** every PR that adds or changes a command, a setting, a secret or the architecture updates `readme.md` in the same PR.
- Tests: xUnit in `tests/trevor.Tests` (bot, referencing `src/trevor/trevor.csproj`) and `tests/Lol.Shared.Tests` (Riot and Data Dragon clients). Build interactions from Discord-shaped JSON with `TestInteractions`.
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
