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
(reported match ids are remembered); remakes are skipped. Needs `DISCORD_URL_MATCHES`.

Every message the bot sends blocks `@everyone`/role/user pings, and if the AI is down, Trevor falls back to his canned lines.

### 🌐 Website (work in progress)
A React site for builds and stats. The **home page** has Trevor's quote of the day, the champion of the day (splash art, lore,
"make a build"), this week's free rotation and, for logged-in users, the *Ranking zmarnowanego czasu* (linked players by
mastery points and account level, with their Solo/Duo rank; Riot publishes no hours played) and the team's newest builds. Builds live under `/buildy`:
a searchable champion picker (names and titles in Polish, icons from Data Dragon),
**Log in with Discord** (the same Discord id the bot sees; with `DISCORD_GUILD_ID` set only members of Trevor's server get in,
shown with their server nickname and avatar; after the first consent, logging in again skips Discord's consent screen) and the **build editor**:
six items in buy order, a full rune page (keystone, primary and secondary runes with League's rules), two summoner spells and the
skill order for levels 1–18 (impossible orders can't be clicked; the max order, e.g. Q > E > W, is shown).
Logged in, you **save** builds (up to 50, checked again by the API against the current patch and the game's rules), find them
under **Moje buildy**, and **share** a link (`/b/<id>`): anyone can open it read-only, only you can edit or delete it.
Under **Konto** (click your name) you link your League account: enter your Riot ID, set the default profile icon the site
shows you, click *Sprawdź* — the API checks the icon through Summoner-V4, so nobody can claim someone else's account.
With a linked account, **Statystyki** shows your Solo/Duo and Flex rank (emblem, LP, wins/losses) and your last 20 games: games, winrate and KDA, a table per champion (games, winrate,
KDA, CS/min, damage/min; Summoner's Rift only, no remakes) and the recent games. Each match is fetched from Riot once and kept;
a reload within 2 minutes doesn't ask Riot again.
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

More detail: [`docs/setup.md`](docs/setup.md) (optional settings still to add by hand), [`CLAUDE.md`](CLAUDE.md) (conventions, gotchas), [`docs/ROADMAP.md`](docs/ROADMAP.md) (ideas and patterns), [`docs/riot-api.md`](docs/riot-api.md) (Riot endpoints and rate limits).

---

## ⚙️ Configuration

Azure Function App settings (locally: copy `src/trevor/local.settings.example.json` to `src/trevor/local.settings.json`):

| Setting | Required | What it is |
|---|---|---|
| `DISCORD_API_KEY` | yes | Discord application **public key** (64 hex chars) |
| `OPENAI_URL`, `OPENAI_KEY`, `OPENAI_DEPLOYMENT` | yes | Azure OpenAI endpoint, key and deployment name |
| `RIOT_API_KEY` | no | Riot API key; without it the LoL commands say the key is missing and match tracking is off |
| `RIOT_PLATFORM` | no | Players' server, default `eun1` (EUNE); `euw1` for EUW |
| `DISCORD_URL_MATCHES` | no | Webhook of the channel for tracked players' match cards; without it tracking posts nothing |
| `AzureWebJobsStorage` | yes (Functions host) | Storage account **connection string**: queue, timers and the `trackedplayers` / `reportedmatches` tables |
| `DISCORD_SKIP_SIGNATURE` | local only | `true` disables Discord signature checks for local testing |
| `WEB_API_URL`, `WEB_API_KEY`, `WEB_SITE_URL` | no | The website's API address, the shared key (= GitHub secret `TREVOR_WEB_KEY`) and the site address. Without them `/rank` and `/roast` need a nick and `/build` says the site isn't connected |

GitHub repository secrets used by the deploy workflow:

| Secret | What it is |
|---|---|
| `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID` | OIDC login to Azure |
| `DISCORD_APP_ID`, `DISCORD_BOT_TOKEN` | Register slash commands from `deploy/discord-commands.json` after each deploy (skipped if missing) |
| `TREVOR_WEB_KEY` | Any long random string, shared by the website API (`BOT_API_KEY`, set by `web.yml`) and the bot (`WEB_API_KEY`, set by hand on the Function App Trevor). Without it the bot works without the site |
| `DISCORD_CLIENT_SECRET` | OAuth2 client secret of the same Discord application: website login (`web.yml` passes it, with `DISCORD_APP_ID`, to the API). Without it login answers 503 |
| `RIOT_API_KEY` | The same Riot API key the bot uses, for the website (linking League accounts). Without it linking answers 503 |

GitHub repository **variable** (Settings → Secrets and variables → Actions → Variables):

| Variable | What it is |
|---|---|
| `DISCORD_GUILD_ID` | Id of Trevor's Discord server (Developer Mode → right-click the server → Copy Server ID). Only its members can log in to the website. Without it anyone with Discord can |

The website's API settings (`DISCORD_CLIENT_ID`, `DISCORD_CLIENT_SECRET`, `DISCORD_REDIRECT_URI`, `SITE_URL`, `RIOT_API_KEY`, storage) are set by
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
  (`Trevor.yml`, registers the slash commands too) and, when website files changed, the website (`web.yml`: creates or updates
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
