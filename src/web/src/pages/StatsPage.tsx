import { Link } from 'react-router'
import { ApiRequestError } from '../api/client'
import { errorText } from '../api/errorText'
import { fetchMyStats } from '../api/stats'
import { useApi } from '../api/useApi'
import { LoginLink } from '../features/account/LoginLink'
import { useCurrentUser } from '../features/account/useCurrentUser'
import { PlayerStatsView } from '../features/stats/PlayerStatsView'

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
          <Link className="button gold" to="/konto">
            Podepnij konto
          </Link>
        )}
      </section>
    )
  }

  return <PlayerStatsView stats={stats.data} title={<h2 className="page-title">{stats.data.riotId}</h2>} />
}
