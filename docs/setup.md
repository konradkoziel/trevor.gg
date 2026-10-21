# Ustawienia do zrobienia ręcznie

Wszystko poniżej jest **opcjonalne**: bez tego bot i strona działają, tylko dana funkcja mówi, czego jej brakuje.
Sekretów nie ma w kodzie, bo repo jest publiczne. Po dodaniu czegokolwiek w GitHubie trzeba wdrożyć stronę
(Actions → **Deploy website** → *Run workflow*), bo dopiero wtedy wartości trafiają do Azure.

| # | Gdzie | Nazwa | Wartość | Co włącza | Bez tego |
|---|---|---|---|---|---|
| 1 | GitHub → Settings → Secrets and variables → Actions → **Secrets** | `RIOT_API_KEY` | Ten sam klucz Riot co w ustawieniach Function App Trevor | Podpinanie konta LoL, statystyki, ranking i rotacja na stronie | Te sekcje piszą, że brakuje klucza Riot albo się chowają |
| 2 | GitHub → … → **Variables** | `DISCORD_GUILD_ID` | ID serwera (Discord: tryb dewelopera → prawy klik na serwer → Kopiuj ID serwera) | Na stronę wchodzą tylko ludzie z serwera, z nickiem i avatarem z serwera | Zalogować się może każdy z kontem Discord |
| 3 | GitHub → … → **Secrets** | `TREVOR_WEB_KEY` | Dowolny długi losowy ciąg (generator haseł) | Hasło, którym Trevor przedstawia się API strony | `/build` i `/rank` / `/roast` bez nicku nie działają |
| 4 | Azure → Function App **Trevor** → Settings → Environment variables | `WEB_API_URL` | `https://<nazwa API strony>.azurewebsites.net` (podsumowanie przebiegu *Deploy website*, linia „API”) | Jak 3 | Jak 3 |
| | | `WEB_API_KEY` | Ta sama wartość co `TREVOR_WEB_KEY` | | |
| | | `WEB_SITE_URL` | Adres strony (podsumowanie *Deploy website*, linia „Site”) | | |

Kolejność dla `/build`: najpierw 3, potem wdrożenie strony, potem 4.
Punkt 4 jest ręczny celowo: wdrożenie strony nie zmienia produkcyjnych ustawień bota.
