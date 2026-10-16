# Trevor — roadmapa i architektura

## Ograniczenie, które decyduje o wszystkim

Trevor działa na **HTTP Interactions** w Azure Functions. Bot **reaguje tylko na slash-komendy, przyciski i formularze**.
Nie widzi zwykłych wiadomości, nie reaguje na eventy (ktoś wszedł, ktoś zaczął grać) i nie wejdzie na voice.
Do tego potrzebny byłby Gateway (WebSocket) i stale działający proces, czyli osobny kontener. To zmienia koszt i
złożoność projektu, więc poniższe pomysły celowo mieszczą się w modelu serverless.

## Pomysły — ocena

Skala: wartość dla serwera / nakład (S/M/L) / ryzyko.

| # | Pomysł | Wartość | Nakład | Uwagi |
|---|---|---|---|---|
| 0 | **Kolejka dla komend odroczonych** (fundament) | wysoka | S | Bez tego każda nowa komenda AI/Riot może gubić odpowiedzi |
| 1 | **`/rank <nick>`** — ranga, LP, winrate z Riot API | wysoka | S | Pierwszy kontakt z Riot API, potrzebny klucz developerski / personal |
| 2 | **`/roast <nick>`** — Trevor ocenia ostatni mecz (statystyki z Riot API → OpenAI) | bardzo wysoka | M | Łączy dwie integracje, najbardziej „trevorowy” pomysł |
| 3 | **`/teams` zbalansowane rangą** zamiast losowania | wysoka | M | Wymaga mapowania Discord user → konto Riot (`/link`) |
| 4 | **`/lfg 20:00`** — „gramy?” z przyciskami Wchodzę/Nie | wysoka | M | Pierwsze komponenty (interaction `type 3`), stan w Table Storage |
| 5 | **`/wynik niebiescy`** + `/ranking` — kto ile wygrał w customach | średnia | S | Naturalne domknięcie `/teams` |
| 6 | **`/ask` z pamięcią rozmowy** (kontekst ostatnich N pytań na kanale) | średnia | S | Koszt tokenów rośnie, potrzebny limit |
| 7 | **Limit `/ask` na użytkownika** (np. 20/dzień) | średnia | S | Ochrona budżetu OpenAI; warto razem z #6 |
| 8 | **Zaplanowane wiadomości konfigurowane komendą** (`/przypomnij`, tygodniowe podsumowanie) | średnia | M | Zastępuje hardkodowany timer |
| 9 | **`/champ`** — losowy champion + rola + build „dla jaj” | niska–średnia | S | Statyczne dane z Data Dragon, zero kosztów |
| 10 | Automatyczna rejestracja slash-komend z CI | średnia (dla Ciebie) | S | Definicje komend w kodzie = jedno źródło prawdy |
| ✗ | Muzyka / voice / reagowanie na wiadomości | — | L | Wymaga Gateway — poza modelem serverless |
| ✗ | Generowanie obrazków/memów | niska | M | Koszt per wywołanie, łatwo przepalić budżet |

### Rekomendowana kolejność

1. ~~**#0 kolejka + porządki**~~ ✅ **zrobione**: kolejka `deferred-commands`, Options z walidacją, rejestr komend, obsługa błędów w `InteractionHandler`, testy.
2. **#1 `/rank`**, potem **#2 `/roast`**. Największa frajda przy małym ryzyku, buduje klienta Riot API.
3. **#4 `/lfg`** i **#5 `/wynik`**. Wprowadzają przyciski i trwały stan.
4. **#3 balans drużyn**. Korzysta z Riot API (#1) i z powiązań kont.
5. Reszta według ochoty. **#7 limit** warto dodać od razu, gdy `/ask` dostanie pamięć (#6).

## Architektura docelowa

> **Stan:** fundament jest wdrożony: rejestr komend w DI, kolejka, Options z walidacją, typowany `DiscordApiClient`, vertical slices i testy.
> Pozostałe wzorce (Strategy dla drużyn, Repository, Cache-Aside, Typed Riot client z resilience, router komponentów) wchodzą razem z funkcjami,
> które ich potrzebują. Dodane wcześniej byłyby pustymi abstrakcjami.
>
> Weryfikacja podpisu została w funkcji HTTP zamiast w `IFunctionsWorkerMiddleware`. Jest tylko jeden endpoint HTTP, a middleware
> musiałby buforować body requestu, czego nie da się tu przetestować lokalnie. Kiedy dojdzie drugi endpoint, warto to przenieść.

Trevor to mała aplikacja, ~1k linii. **Clean Architecture z czterema projektami, MediatR, CQRS i event sourcingiem
byłyby przerostem formy.** Wystarczy jedna Function App podzielona na **vertical slices** (folder per feature) plus
kilka dobrze dobranych wzorców w miejscach, które realnie bolą.

```
                ┌──────────────── Function App "Trevor" ─────────────────┐
Discord ──HTTP──▶ DiscordInteractions (HTTP)                              │
                │   middleware: podpis → logowanie → mapowanie błędów    │
                │   ├─ PING → PONG                                        │
                │   ├─ szybka komenda → odpowiedź type 4                  │
                │   └─ wolna komenda → type 5 + wiadomość na kolejkę ──┐  │
                │                                                     ▼  │
                │ DeferredCommands (QueueTrigger) ─▶ OpenAI / Riot API   │
                │   └─▶ followup do Discorda (retry, poison queue)       │
                │ Timers (TimerTrigger) ─▶ zaplanowane wiadomości        │
                └───────────────┬───────────────────────────────────────┘
                                ▼
                   Azure Storage: Queue + Table (linki kont, LFG, wyniki, limity)
```

### Wzorce — co, gdzie i dlaczego

| Wzorzec | Gdzie | Dlaczego |
|---|---|---|
| **Command + rejestr przez DI** (zamiast `switch` w `CommandFactory`) | Każda komenda implementuje `ICommand` z `Name` i definicją opcji; rejestracja `services.AddSingleton<ICommand, RankCommand>()`, dispatcher buduje słownik z `IEnumerable<ICommand>` | Nowa komenda = nowa klasa, zero edycji w fabryce (Open/Closed). Ta sama definicja służy do rejestracji w Discordzie (#10) |
| **Queue-Based Load Leveling** | Wolne komendy: HTTP odpowiada `type 5`, wrzuca `{commandName, interaction}` na Storage Queue, `QueueTrigger` wykonuje i wysyła followup | Gwarantowane dostarczenie, retry i poison queue za darmo; usuwa `Task.Run`. Storage już masz |
| **Middleware / Chain of Responsibility** (`IFunctionsWorkerMiddleware`) | Weryfikacja podpisu, logowanie, zamiana wyjątków na odpowiedź w stylu Trevora, rate limit `/ask` | Funkcja HTTP robi tylko routing; logika przekrojowa w jednym miejscu, łatwa do testów |
| **Typed HttpClient + Anti-Corruption Layer** | `DiscordApiClient`, `RiotApiClient` (z `Microsoft.Extensions.Http.Resilience`: retry, rate limit, timeout) | Riot API ma twarde limity (20 req/s, 100/2 min na kluczu dev). Reszta kodu operuje na własnych modelach (`PlayerRank`), nie na JSON-ie Riota |
| **Cache-Aside** | Rangi i ostatnie mecze (`IMemoryCache` lub Table Storage, TTL ~10 min) | Oszczędza limity Riot API przy `/teams` dla 10 graczy |
| **Strategy** | `ITeamSplitStrategy`: `RandomSplit` (obecne) i `RankBalancedSplit` | `/teams` wybiera strategię opcją komendy; łatwy do testów algorytm |
| **Repository** (cienki, nad Azure Table Storage) | `PlayerLinkRepository`, `LfgRepository`, `MatchResultRepository`, `UsageRepository` | Tani i wystarczający storage. Interfejs pozwala testować komendy bez Azure |
| **Router komponentów po `custom_id`** | Przyciski LFG: `custom_id = "lfg:join:{id}"`, handler wybrany po prefiksie — ten sam mechanizm co rejestr komend | Komponenty (`type 3`) obsługiwane tak samo jak komendy, bez osobnego `if`-a na każdy przycisk |
| **Options pattern** z walidacją na starcie | `DiscordOptions`, `OpenAiOptions`, `RiotOptions` + `ValidateDataAnnotations().ValidateOnStart()` | Zamiast rozsianych `Environment.GetEnvironmentVariable` i wyjątków przy pierwszym requeście |
| **Prompt jako konfiguracja** | Persona Trevora w jednym pliku/stałej, osobne szablony dla `/ask` i `/roast` | Jeden charakter bota, łatwiej stroić bez szukania stringów po kodzie |

### Czego świadomie NIE robić

- **MediatR / CQRS**: rejestr komend robi dokładnie to samo bez dodatkowej paczki.
- **Cosmos DB / SQL**: Table Storage kosztuje grosze i wystarcza dla jednego serwera.
- **Osobny serwis na Gateway**: dopiero wtedy, gdy naprawdę będzie potrzebny voice albo reagowanie na wiadomości.
- **Mikroserwisy**: jedna Function App, kilka funkcji.

### Struktura folderów (docelowo)

```
Features/
  Ask/        AskCommand.cs, TrevorPrompts.cs
  Teams/      TeamsCommand.cs, ITeamSplitStrategy.cs, RandomSplit.cs, RankBalancedSplit.cs
  Lol/        RankCommand.cs, RoastCommand.cs, LinkCommand.cs
  Lfg/        LfgCommand.cs, LfgButtonHandler.cs
Infrastructure/
  Discord/    DiscordApiClient.cs, SignatureMiddleware.cs, Dtos/
  Riot/       RiotApiClient.cs, Dtos/
  Storage/    *Repository.cs
Functions/    DiscordInteractions.cs, DeferredCommands.cs, Timers.cs
tests/        trevor.Tests (xUnit): strategie drużyn, parsowanie opcji, router, podpis
```

Do budowy dochodzi **projekt testów** podpięty pod `build.yml`, żeby PR sprawdzał coś więcej niż kompilację.
