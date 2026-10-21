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

### `/rank nick: Nick#EUNE`
Pulls Solo/Duo and Flex rank (tier, LP, wins/losses, winrate) from the **Riot API** and shows it as a Discord **embed**:
the ranked emblem of your main queue (Solo/Duo, else Flex), a bar in the tier's colour, and both queues in columns.
Trevor reads your LP and writes a fresh AI verdict on it every time.

### `/roast nick: Nick#EUNE`
Fetches the player's **last match** and shows it as a Discord **embed**: champion icon, a green/red bar for win/loss,
and KDA, CS/min, damage share, kill participation, vision and gold in columns — topped off with Trevor's AI roast.
Played well? He'll still find something.

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
```

- **Vertical slices:** each command lives in `Features/<Name>/` and implements `ICommand`; commands are picked up from DI by name.
- **Infrastructure:** `Infrastructure/Discord` (models, signature verification, API client), `Infrastructure/Riot` (typed client with retries on 429/5xx), `Infrastructure/OpenAi` (`ITrevorAi` — Trevor's persona and prompts in one place), `Infrastructure/Storage` (tracked players in Azure Table Storage).
- **Queue-based deferral:** Discord needs an answer within 3 s; slow work goes through the `deferred-commands` queue and is retried if posting to Discord fails.

More detail: [`CLAUDE.md`](CLAUDE.md) (conventions, gotchas), [`docs/ROADMAP.md`](docs/ROADMAP.md) (ideas and patterns), [`docs/riot-api.md`](docs/riot-api.md) (Riot endpoints and rate limits).

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

GitHub repository secrets used by the deploy workflow:

| Secret | What it is |
|---|---|
| `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID` | OIDC login to Azure |
| `DISCORD_APP_ID`, `DISCORD_BOT_TOKEN` | Register slash commands from `deploy/discord-commands.json` after each deploy (skipped if missing) |

---

## 🛠️ Development

```
src/trevor/            Function App: Functions/, Core/, Features/, Infrastructure/, Program.cs, host.json
tests/trevor.Tests/    xUnit tests
deploy/                discord-commands.json (registered with Discord on every deploy)
docs/                  roadmap, Riot API notes
```

```bash
dotnet build -c Release
dotnet test -c Release --no-build
```

- **Adding a command:** create `src/trevor/Features/<Name>/<Name>Command.cs` implementing `ICommand`, register it in `Program.cs`, add its definition to `deploy/discord-commands.json` and write tests in `tests/trevor.Tests`. A test fails if the JSON and the code drift apart.
- **Deploy:** open a PR to `main` (CI builds and tests it). Merging deploys to Azure and registers the slash commands automatically.

---

## 🧰 Tech Stack

- **Azure Functions (.NET 10, isolated worker)** — webhook-based backend for Discord Interactions
- **Azure Storage Queue** — reliable deferred responses for slow commands
- **Azure Table Storage** — tracked players and the matches already reported
- **Azure OpenAI Service** — powers `/ask`, the `/rank` verdict, `/roast`, `Rate this` and match card comments
- **Riot API** — Account-V1, League-V4, Match-V5; **Data Dragon** for champion icons
- **Discord Interactions API** — slash commands with Ed25519 signature verification
- **C#** — DI, options validated at startup, typed HTTP clients with resilience, xUnit tests
- **Trevor Personality Engine™** — sarcasm-driven orchestration layer
