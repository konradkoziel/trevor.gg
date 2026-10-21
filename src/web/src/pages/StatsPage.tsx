import { Link } from 'react-router'
import { ApiRequestError } from '../api/client'
import { errorText } from '../api/errorText'
import { fetchMyStats } from '../api/stats'
import { useApi } from '../api/useApi'
import { LoginLink } from '../features/account/LoginLink'
import { useCurrentUser } from '../features/account/useCurrentUser'
import { ChampionTable } from '../features/stats/ChampionTable'
import { formatDecimal, formatPercent, winRateTone } from '../features/stats/format'
import { RecentMatches } from '../features/stats/RecentMatches'

export function StatsPage() {
  const { state } = useCurrentUser()

  if (state.status === 'loading') return <p className="muted">…</p>
  if (state.status === 'anonymous')
    return (
      <section className="panel">
        <p>Statystyki są dla zalogowanych. Trevor musi wiedzieć, czyje porażki liczy.</p>
        <LoginLink />
      </section>
    )
  return <Stats />
}

function Stats() {
  const stats = useApi(fetchMyStats)

  if (stats.status === 'loading')
    return <p className="muted">Liczę Twoje porażki… Pierwszy raz może to potrwać kilka sekund.</p>
  if (stats.status === 'error') {
    const notLinked = stats.error instanceof ApiRequestError && stats.error.status === 404
    return (
      <section className="panel">
        <p className="error">{errorText(stats.error, 'Nie udało się pobrać statystyk.')}</p>
        {notLinked && (
          <Link className="button discord" to="/konto">
            Podepnij konto
          </Link>
        )}
      </section>
    )
  }

  const s = stats.data
  return (
    <>
      <section className="stats-summary">
        <h2 className="page-title">{s.riotId}</h2>
        <div className="summary-tiles">
          <div className="tile">
            <span className="tile-value">{s.games}</span>
            <span className="muted">gier na Rifcie</span>
          </div>
          <div className="tile">
            <span className={`tile-value ${winRateTone(s.winRate)}`}>{formatPercent(s.winRate)}</span>
            <span className="muted">
              winrate ({s.wins}W / {s.games - s.wins}L)
            </span>
          </div>
          <div className="tile">
            <span className="tile-value">{formatDecimal(s.kda)}</span>
            <span className="muted">KDA</span>
          </div>
        </div>
        <p className="muted">
          Ostatnie mecze z Riot API, bez remake'ów. Statystyki championów tylko z Summoner's Rift.
        </p>
      </section>

      {s.champions.length > 0 && (
        <section className="home-section">
          <h2 className="page-title">Championy</h2>
          <ChampionTable champions={s.champions} />
        </section>
      )}
      <section className="home-section">
        <h2 className="page-title">Ostatnie gry</h2>
        {s.recent.length > 0 ? (
          <RecentMatches matches={s.recent} />
        ) : (
          <p className="muted">Brak gier. Może czas zagrać?</p>
        )}
      </section>
    </>
  )
}
