# Ustawienia

Każde ustawienie ma **jedną nazwę wszędzie**: sekret w GitHubie o tej nazwie, a wdrożenia same wpisują go do Azure
(`Deploy bot` do Function App Trevor, `Deploy website` do API strony). Niczego nie wpisujesz ręcznie w Azure.

Gdzie: GitHub → Settings → Secrets and variables → Actions → **Secrets** (albo **Variables**, gdzie tak napisane).
Po zmianie sekretu uruchom wdrożenie, którego dotyczy (Actions → *Deploy bot* / *Deploy website* → *Run workflow*):
wartości trafiają do Azure dopiero przy wdrożeniu.

| Nazwa | Bot | Strona | Wymagane | Wartość |
|---|---|---|---|---|
| `DISCORD_PUBLIC_KEY` | ✔ | | tak | Discord Developer Portal → aplikacja → General Information → *Public Key* |
| `OPENAI_URL` | ✔ | | tak | Adres zasobu Azure OpenAI (`https://<nazwa>.openai.azure.com/`) |
| `OPENAI_KEY` | ✔ | | tak | Klucz zasobu Azure OpenAI (Keys and Endpoint) |
| `OPENAI_DEPLOYMENT` | ✔ | | tak | Nazwa wdrożenia modelu w Azure OpenAI |
| `DISCORD_MATCHES_WEBHOOK` | ✔ | | nie | Webhook kanału na raporty meczów (kanał → Integracje → Webhooki). Bez niego `/track` nic nie wrzuca |
| `RIOT_API_KEY` | ✔ | ✔ | nie | Klucz z developer.riotgames.com. Deweloperski wygasa po 24 h, lepiej Personal API Key |
| `TREVOR_WEB_KEY` | ✔ | ✔ | nie | Dowolny długi losowy ciąg. Bez niego `/build` i `/rank` / `/roast` bez nicku nie działają |
| `DISCORD_APP_ID` | ✔ | ✔ | nie | Developer Portal → *Application ID*. Rejestracja komend bota i logowanie na stronie |
| `DISCORD_BOT_TOKEN` | ✔ | | nie | Developer Portal → Bot → *Token*. Rejestracja komend po wdrożeniu |
| `DISCORD_CLIENT_SECRET` | | ✔ | nie | Developer Portal → OAuth2 → *Client Secret*. Logowanie na stronie |
| `DISCORD_GUILD_ID` (**Variable**) | | ✔ | nie | ID serwera (tryb dewelopera → prawy klik na serwer → Kopiuj ID). Na stronę wchodzą tylko ludzie z serwera |
| `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID` | ✔ | ✔ | tak | Logowanie wdrożeń do Azure (OIDC) |

Adresy strony i API (`WEB_API_URL`, `WEB_SITE_URL`) bot dostaje sam z ostatniego wdrożenia strony.
`Deploy bot` zatrzymuje się na starcie, jeśli brakuje któregoś wymaganego sekretu: wtedy nic nie jest wdrażane.

Komunikat „Riot odrzucił klucz API” znaczy, że klucz jest, ale nieważny (najczęściej wygasł). Podmień `RIOT_API_KEY`
i uruchom oba wdrożenia.

## Przeniesienie ze starych nazw (jednorazowo)

Dotąd część ustawień bota była wpisana ręcznie w Azure → Function App Trevor → Environment variables. Skopiuj je do sekretów:

| Stara nazwa w Azure | Nowy sekret w GitHubie |
|---|---|
| `DISCORD_API_KEY` | `DISCORD_PUBLIC_KEY` |
| `DISCORD_URL_MATCHES` | `DISCORD_MATCHES_WEBHOOK` |
| `OPENAI_URL`, `OPENAI_KEY`, `OPENAI_DEPLOYMENT` | te same nazwy |

`Deploy bot` usuwa stare nazwy z Azure sam, po wdrożeniu kodu, który czyta nowe (także `WEB_API_KEY`).
