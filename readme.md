# 🧠 TrevorBot — Discord's Operational Diagnostician

TrevorBot is a Discord bot built on **Azure Functions**, **Azure OpenAI** and the **Riot API**, designed for teams that thrive on chaos, rituals, and judgmental automation.
He doesn’t just respond — he observes, analyzes, and occasionally helps.

---

## 🚀 Features

### `/ping`
Checks if Trevor is alive.
He usually is. Still cynical.

### `/teams gracze: Gracz1,Gracz2,Gracz3,Gracz4`
Splits the provided list of players into two teams.
If they’re uneven — Trevor sees all. And judges.

### `/ask question: <text>`
Connects directly to **Azure OpenAI**.
Trevor reads your question, consults the neural gods, and answers briefly and precisely.
He’s helpful by contract, but not necessarily polite.

### `/rank nick: Nick#EUNE` (or just `/rank`)
Pulls Solo/Duo and Flex rank (tier, LP, wins/losses, winrate) from the **Riot API** and shows it as a Discord **embed**:
the ranked emblem of your main queue (Solo/Duo, else Flex), a bar in the tier's colour, and both queues in columns.
Trevor reads your LP and writes a fresh AI verdict on it every time.

### `/roast nick: Nick#EUNE` (or just `/roast`)
Fetches the player's **last match** and shows it as a Discord **embed**: champion icon, a green/red bar for win/loss,
and KDA, CS/min, damage share, kill participation, vision and gold in columns — topped off with Trevor's AI roast.
Played well? He'll still find something.

Without a nick, `/rank` and `/roast` use the League account you linked on the website.

### `/build champion: Yasuo`
Your newest build for that champion from the website, as an embed: items in buy order, runes, summoner spells, the skill max
order and a link to the build on the site. Typing is forgiving: `yasuo`, `Kai'Sa`, `kaisa`, `wukong` all work.

### Right-click a message → Apps → `Rate this`
A message context-menu command: Trevor quotes the message and judges it (meaning, cringe level, spelling, maybe a 1–10 score).
Works on any text message on the server; images and embeds without text get a snarky refusal instead.

### `/track nick: Nick#EUNE` and `/untrack nick: Nick#EUNE`
Puts a player on (or off) Trevor's watch list, up to 20 players. Every 5 minutes he checks their matches, and after each
finished game posts a short card to a dedicated matches channel: result, queue, length, every tracked player's champion and KDA,
the team's **🏆 MVP** (best KDA, with the champion's icon) and **🚨 report candidate** (worst KDA). Tracked players count too,
so a friend can end up on either line. No AI here — just the numbers.
Friends in the same game get one shared card, even when Riot lists the match for one of them a few minutes later
(reported match ids are remembered); remakes are skipped. Needs `DISCORD_MATCHES_WEBHOOK`.

Every message the bot sends blocks `@everyone`/role/user pings, and if the AI is down, Trevor falls back to his canned lines.

### 🌐 Website (work in progress)
A React site for builds and stats. The **home page** has Trevor's quote of the day, the champion of the day (splash art, lore,
"make a build"), this week's free rotation and, for logged-in users, the *Ranking zmarnowanego czasu* (linked players by
mastery points and account level, with their Solo/Duo rank; Riot publishes no hours played) and the team's newest builds. Builds live under `/buildy`:
a searchable champion picker (names and titles in Polish, icons from Data Dragon),
**Log in with Discord** (the same Discord id the bot sees; with `DISCORD_GUILD_ID` set only members of Trevor's server get in,
shown with their server nickname and avatar; after the first consent, logging in again skips Discord's consent screen) and the **build editor**:
six items in buy order, a full rune page (keystone, primary and secondary runes with League's rules), two summoner spells and the
max order (click e.g. Q, E, W; the level-by-level order with the ultimate at 6/11/16 follows from it).
Logged in, you **save** builds (up to 50, checked again by the API against the current patch and the game's rules), find them
under **Moje buildy**, and **share** a link: anyone can open it read-only (`/b/<id>`), only you can edit or delete it.
The copied link goes through the API (`/api/share/<id>`), so on Discord it unfolds into a preview with the build's name,
items, runes and the champion's splash art, then sends people on to the site.
Under **Konto** (click your name) you link your League account by Riot ID. There is no ownership check: only members of
Trevor's server can log in, and an account someone else already linked is refused.
With a linked account, **Statystyki** shows, in two columns, your Solo/Duo and Flex rank (big emblem, LP towards the next
division, this season's record and winrate ring from Riot) and the champions of your recent games next to the last 20 games, one line
each (score, KDA ratio, CS/min, length; Summoner's Rift only in the totals, no remakes). Each match is fetched from Riot once and kept;
a reload within 2 minutes doesn't ask Riot again.
Every linked friend has a **profile** at `/gracz/<name>/<tag>` (click a name in the leaderboard; logged-in users only):
rank, level, the same stats, their five most played champions by mastery and their builds.
Next: `/build` in Trevor.

---

## 🏗️ How it works

```
Discord ──HTTP──▶ DiscordCommands (HTTP function: signature check)
                    ├─ fast commands (/ping, /teams) → answered right away
                    └─ slow commands (/ask, /rank, /roast, Rate this) → "Trevor is thinking…" + message on a Storage Queue
                                                                   │
                  DeferredCommands (queue function) ◀──────────────┘
                    └─ runs the command (Azure OpenAI / Riot API) → posts the answer back to Discord

MatchTracker (timer, every 5 min) → tracked players (Table Storage) → new matches (Riot API) → card on the matches channel (webhook)

Browser ──▶ Static Web App "trevor-web" (React, src/web)
        └─fetch─▶ Function App "trevor-web-api-*" (src/Web.Api, CORS: only the site) ──▶ Data Dragon / Riot API

Login: site ─▶ /api/auth/discord/login ─▶ discord.com (Authorize) ─▶ /api/auth/discord/callback ─▶ site/#session=<token>
       then every API call sends "Authorization: Bearer <token>" (signed JWT, 7 days)
```

- **Vertical slices:** each command lives in `Features/<Name>/` and implements `ICommand`; commands are picked up from DI by name.
- **Infrastructure:** `Infrastructure/Discord` (models, signature verification, API client), `Infrastructure/OpenAi` (`ITrevorAi` — Trevor's persona and prompts in one place), `Infrastructure/Storage` (tracked players in Azure Table Storage).
- **Shared LoL code:** `src/Lol.Shared` — Riot API typed client with retries on 429/5xx, Data Dragon, models; `AddRiot()` wires it into any app in the repo.
- **Website:** separate resource group `trevor-web`, created from `infra/web.bicep` by its own workflow; the bot is never redeployed for a website-only change.
- **Queue-based deferral:** Discord needs an answer within 3 s; slow work goes through the `deferred-commands` queue and is retried if posting to Discord fails.

More detail: [`docs/setup.md`](docs/setup.md) (every secret to set, in one table), [`CLAUDE.md`](CLAUDE.md) (conventions, gotchas), [`docs/ROADMAP.md`](docs/ROADMAP.md) (ideas and patterns), [`docs/riot-api.md`](docs/riot-api.md) (Riot endpoints and rate limits).

---

## ⚙️ Configuration

Every setting has one name everywhere: a **GitHub secret** of that name, which the deploy workflows put into Azure
(`bot.yml` into the Function App Trevor, `web.yml` into the website's API). Nothing is typed into Azure by hand.
The full list with where each value comes from: [`docs/setup.md`](docs/setup.md).

The bot's app settings (locally: copy `src/trevor/local.settings.example.json` to `src/trevor/local.settings.json`):

| Setting | Required | What it is |
|---|---|---|
| `DISCORD_PUBLIC_KEY` | yes | Discord application **public key** (64 hex chars) |
| `OPENAI_URL`, `OPENAI_KEY`, `OPENAI_DEPLOYMENT` | yes | Azure OpenAI endpoint, key and deployment name |
| `RIOT_API_KEY` | no | Riot API key; without it the LoL commands say the key is missing and match tracking is off |
| `RIOT_PLATFORM` | no | Players' server, default `eun1` (EUNE); `euw1` for EUW |
| `DISCORD_MATCHES_WEBHOOK` | no | Webhook of the channel for tracked players' match cards; without it tracking posts nothing |
| `AzureWebJobsStorage` | yes (Functions host) | Storage account **connection string**: queue, timers and the `trackedplayers` / `reportedmatches` tables |
| `DISCORD_SKIP_SIGNATURE` | local only | `true` disables Discord signature checks for local testing |
| `TREVOR_WEB_KEY` | no | The key the bot and the website's API share. Without it `/rank` and `/roast` need a nick and `/build` says the site isn't connected |
| `WEB_API_URL`, `WEB_SITE_URL` | no | The website's API and site addresses; `bot.yml` reads them from the website's last deployment |

`AzureWebJobsStorage`, `RIOT_PLATFORM` and `DISCORD_SKIP_SIGNATURE` are not managed by the workflow.

GitHub repository secrets that are not app settings, or are shared by both apps:

| Secret | What it is |
|---|---|
| `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID` | OIDC login to Azure |
| `DISCORD_APP_ID`, `DISCORD_BOT_TOKEN` | Register slash commands from `deploy/discord-commands.json` after each deploy (skipped if missing) |
| `TREVOR_WEB_KEY` | Any long random string, the same app setting on the bot and the website's API. Without it the bot works without the site |
| `DISCORD_CLIENT_SECRET` | OAuth2 client secret of the same Discord application: website login (`web.yml` passes it and `DISCORD_APP_ID` to the API). Without it login answers 503 |
| `RIOT_API_KEY` | One Riot API key for both apps (their rate limits add up). Without it the LoL features say the key is missing |

GitHub repository **variable** (Settings → Secrets and variables → Actions → Variables):

| Variable | What it is |
|---|---|
| `DISCORD_GUILD_ID` | Id of Trevor's Discord server (Developer Mode → right-click the server → Copy Server ID). Only its members can log in to the website. Without it anyone with Discord can |

The website's API settings (`DISCORD_APP_ID`, `DISCORD_CLIENT_SECRET`, `DISCORD_REDIRECT_URI`, `SITE_URL`, `RIOT_API_KEY`, storage) are set by
`infra/web.bicep`, not by hand. The redirect URL Discord must allow (Developer Portal → OAuth2 → Redirects) is printed in the
`Deploy website` run summary.

---

## 🛠️ Development

```
src/trevor/            Function App: Functions/, Core/, Features/, Infrastructure/, Program.cs, host.json
src/Lol.Shared/        class library shared by the apps: Riot API client, Data Dragon, models
tests/trevor.Tests/    xUnit tests of the bot
tests/Lol.Shared.Tests/ xUnit tests of the shared Riot code
src/Web.Api/           Function App with the website's API (champions, build data, abilities, Discord login, /me, builds)
tests/Web.Api.Tests/   xUnit tests of the API
src/web/               website: React + Vite + TypeScript (npm run dev / test / lint / build)
infra/web.bicep        Azure resources of the website
deploy/                discord-commands.json (registered with Discord on every deploy)
docs/                  roadmap, Riot API notes
```

```bash
dotnet build -c Release
dotnet test -c Release --no-build
```

- **Adding a command:** create `src/trevor/Features/<Name>/<Name>Command.cs` implementing `ICommand`, register it in `Program.cs`, add its definition to `deploy/discord-commands.json` and write tests in `tests/trevor.Tests`. A test fails if the JSON and the code drift apart.
- **Deploy:** open a PR to `main` (CI builds and tests the bot, the API and the site, and checks the Bicep). Merging deploys the bot
  (`bot.yml`, registers the slash commands too) and, when website files changed, the website (`web.yml`: creates or updates
  the Azure resources in `trevor-web`, then deploys the API and the site). The site's URL is in the run's summary.
- **Website locally:** `cd src/web && npm ci && npm run dev`; with the API running (`func start` in `src/Web.Api`, settings from
  `local.settings.example.json`) Vite proxies `/api` to it.

---

## 🧰 Tech Stack

- **Azure Functions (.NET 10, isolated worker)** — webhook-based backend for Discord Interactions
- **Azure Storage Queue** — reliable deferred responses for slow commands
- **React + Vite + TypeScript** on **Azure Static Web Apps** — the website; its API on a Flex Consumption Function App; infrastructure in **Bicep**
- **Azure Table Storage** — tracked players and the matches already reported
- **Azure OpenAI Service** — powers `/ask`, the `/rank` verdict, `/roast` and `Rate this`
- **Riot API** — Account-V1, League-V4, Match-V5; **Data Dragon** for champion icons
- **Discord Interactions API** — slash commands with Ed25519 signature verification
- **C#** — DI, options validated at startup, typed HTTP clients with resilience, xUnit tests
- **Trevor Personality Engine™** — sarcasm-driven orchestration layer
