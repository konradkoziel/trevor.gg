# Riot API — dostępne metody (Development Key)

Lista z Riot Developer Portal dla klucza deweloperskiego tej aplikacji (59 metod), stan na 2026-10-17.
Limity podane przy metodach to **limity per metoda**. Niezależnie od nich klucz ma **limit aplikacji**
(z Developer Portal): **20 zapytań / 1 s i 100 zapytań / 2 min**, liczone **osobno dla każdej wartości routingu**
(`eun1`, `euw1`, `europe`, `americas`, …). `/rank` robi 1 zapytanie na `europe` i 1 na `eun1`, więc jedno wywołanie
zużywa po 1 z obu pul. Klucz deweloperski wygasa po 24 h.

## Routing

- **Platform** (`eun1`, `euw1`, …): `summoner`, `league`, `champion-mastery`, `spectator`, `lol-status`, `clash`, `lol-challenges`, `champion-v3`.
- **Regional** (`europe`, `americas`, `asia`, `sea`): `account-v1`, `match-v5` (dla `match-v5` serwery EU → `europe`).
- W kodzie: `src/trevor/Infrastructure/Riot/RiotRouting.cs` (`RIOT_PLATFORM`, domyślnie `eun1`).

## Co jest używane i do czego się przyda

| Metoda | Do czego | Stan |
|---|---|---|
| `account-v1` `GET /riot/account/v1/accounts/by-riot-id/{gameName}/{tagLine}` | Riot ID (`Nick#TAG`) → PUUID | ✅ `/rank` |
| `league-v4` `GET /lol/league/v4/entries/by-puuid/{encryptedPUUID}` | Rangi Solo/Duo i Flex | ✅ `/rank` |
| `match-v5` `GET /lol/match/v5/matches/by-puuid/{puuid}/ids` | Ostatnie mecze gracza | `/roast` (#2) |
| `match-v5` `GET /lol/match/v5/matches/{matchId}` | Statystyki meczu (KDA, CS, obrażenia) | `/roast` (#2) |
| `spectator-v5` `GET /lol/spectator/v5/active-games/by-summoner/{encryptedPUUID}` | Czy gracz właśnie gra (pomimo nazwy przyjmuje PUUID) | pomysł: `/gra <nick>`, LFG |
| `champion-mastery-v4` `GET …/champion-masteries/by-puuid/{encryptedPUUID}/top` | Najczęściej grane postacie | pomysł: „jednotrikowiec” w `/roast` |
| `champion-v3` `GET /lol/platform/v3/champion-rotations` | Darmowa rotacja | pomysł: `/rotacja`, tygodniowy post |
| `lol-status-v4` `GET /lol/status/v4/platform-data` | Awarie serwerów | pomysł: „to nie lag, to Riot” |
| `clash-v1` | Drużyny i turnieje Clash | pomysł na później |

## Pełna lista (dla LoL)

### account-v1 (regional)
| Metoda | Limit |
|---|---|
| `GET /riot/account/v1/accounts/by-riot-id/{gameName}/{tagLine}` | 1000 / 1 min (druga pozycja na liście: 20000 / 10 s, 1 200 000 / 10 min) |
| `GET /riot/account/v1/accounts/by-puuid/{puuid}` | 1000 / 1 min (druga pozycja na liście: 20000 / 10 s, 1 200 000 / 10 min) |
| `GET /riot/account/v1/active-shards/by-game/{game}/by-puuid/{puuid}` | 20000 / 10 s, 1 200 000 / 10 min |
| `GET /riot/account/v1/region/by-game/{game}/by-puuid/{puuid}` | 20000 / 10 s, 1 200 000 / 10 min |

### summoner-v4
| Metoda | Limit |
|---|---|
| `GET /lol/summoner/v4/summoners/by-puuid/{encryptedPUUID}` | 1600 / 1 min |
| `GET /lol/summoner/v4/summoners/me` | 20000 / 10 s, 1 200 000 / 10 min |

### league-v4
| Metoda | Limit |
|---|---|
| `GET /lol/league/v4/entries/by-puuid/{encryptedPUUID}` | 20000 / 10 s, 1 200 000 / 10 min |
| `GET /lol/league/v4/entries/{queue}/{tier}/{division}` | 50 / 10 s |
| `GET /lol/league/v4/challengerleagues/by-queue/{queue}` | 30 / 10 s, 500 / 10 min |
| `GET /lol/league/v4/grandmasterleagues/by-queue/{queue}` | 30 / 10 s, 500 / 10 min |
| `GET /lol/league/v4/masterleagues/by-queue/{queue}` | 30 / 10 s, 500 / 10 min |

### league-exp-v4
| Metoda | Limit |
|---|---|
| `GET /lol/league-exp/v4/entries/{queue}/{tier}/{division}` | 50 / 10 s |

### match-v5 (regional)
| Metoda | Limit |
|---|---|
| `GET /lol/match/v5/matches/by-puuid/{puuid}/ids` | 2000 / 10 s |
| `GET /lol/match/v5/matches/{matchId}` | 2000 / 10 s |
| `GET /lol/match/v5/matches/{matchId}/timeline` | 2000 / 10 s |
| `GET /lol/match/v5/matches/by-puuid/{puuid}/replays` | 20000 / 10 s, 1 200 000 / 10 min |

### spectator-v5
| Metoda | Limit |
|---|---|
| `GET /lol/spectator/v5/active-games/by-summoner/{encryptedPUUID}` | 3000 / 10 s, 180 000 / 10 min |

### champion-mastery-v4
| Metoda | Limit |
|---|---|
| `GET /lol/champion-mastery/v4/champion-masteries/by-puuid/{encryptedPUUID}` | 20000 / 10 s, 1 200 000 / 10 min |
| `GET /lol/champion-mastery/v4/champion-masteries/by-puuid/{encryptedPUUID}/by-champion/{championId}` | 20000 / 10 s, 1 200 000 / 10 min |
| `GET /lol/champion-mastery/v4/champion-masteries/by-puuid/{encryptedPUUID}/top` | 20000 / 10 s, 1 200 000 / 10 min |
| `GET /lol/champion-mastery/v4/scores/by-puuid/{encryptedPUUID}` | 20000 / 10 s, 1 200 000 / 10 min |

### champion-v3
| Metoda | Limit |
|---|---|
| `GET /lol/platform/v3/champion-rotations` | 20000 / 10 s, 1 200 000 / 10 min |

### lol-challenges-v1
| Metoda | Limit |
|---|---|
| `GET /lol/challenges/v1/challenges/config` | 20000 / 10 s, 1 200 000 / 10 min |
| `GET /lol/challenges/v1/challenges/percentiles` | 20000 / 10 s, 1 200 000 / 10 min |
| `GET /lol/challenges/v1/challenges/{challengeId}/config` | 20000 / 10 s, 1 200 000 / 10 min |
| `GET /lol/challenges/v1/challenges/{challengeId}/percentiles` | 20000 / 10 s, 1 200 000 / 10 min |
| `GET /lol/challenges/v1/challenges/{challengeId}/leaderboards/by-level/{level}` | 20000 / 10 s, 1 200 000 / 10 min |
| `GET /lol/challenges/v1/player-data/{puuid}` | 20000 / 10 s, 1 200 000 / 10 min |

### clash-v1
| Metoda | Limit |
|---|---|
| `GET /lol/clash/v1/players/by-puuid/{puuid}` | 20000 / 10 s, 1 200 000 / 10 min |
| `GET /lol/clash/v1/teams/{teamId}` | 200 / 1 min |
| `GET /lol/clash/v1/tournaments` | 10 / 1 min |
| `GET /lol/clash/v1/tournaments/{tournamentId}` | 10 / 1 min |
| `GET /lol/clash/v1/tournaments/by-team/{teamId}` | 200 / 1 min |

### lol-status-v4
| Metoda | Limit |
|---|---|
| `GET /lol/status/v4/platform-data` | 20000 / 10 s, 1 200 000 / 10 min |

### tournament-stub-v5 (testowe turnieje, bez prawdziwych meczów)
| Metoda | Limit |
|---|---|
| `POST /lol/tournament-stub/v5/providers` | 20000 / 10 s, 1 200 000 / 10 min |
| `POST /lol/tournament-stub/v5/tournaments` | 20000 / 10 s, 1 200 000 / 10 min |
| `POST /lol/tournament-stub/v5/codes` | 20000 / 10 s, 1 200 000 / 10 min |
| `GET /lol/tournament-stub/v5/codes/{tournamentCode}` | 20000 / 10 s, 1 200 000 / 10 min |
| `GET /lol/tournament-stub/v5/lobby-events/by-code/{tournamentCode}` | 20000 / 10 s, 1 200 000 / 10 min |

## Pozostałe gry (nieużywane)

- **TFT:** `tft-match-v1` (`matches/{matchId}` 250/10 s, `matches/by-puuid/{puuid}/ids` 600/10 s), `tft-summoner-v1` (`by-puuid` 1600/1 min, `me`), `tft-league-v1` (`entries/{tier}/{division}` 250/10 s; `challenger`, `grandmaster`, `master` 30/10 s, 500/10 min; `rated-ladders/{queue}/top`, `by-puuid/{puuid}`), `tft-status-v1`.
- **LoR:** `lor-ranked-v1` (`leaderboards` 30/10 s, 500/10 min), `lor-match-v1` (`matches/{matchId}` 100/1 h, `matches/by-puuid/{puuid}/ids` 200/1 h), `lor-status-v1`.
- **Valorant:** `val-content-v1` (`contents` 250/10 s), `val-ranked-v1` (`leaderboards/by-act/{actId}` 10/10 s), `val-status-v1`.
