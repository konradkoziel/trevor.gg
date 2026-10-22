import { Link } from 'react-router'
import { ApiRequestError } from '../api/client'
import { errorText } from '../api/errorText'
import { useState } from 'react'
import { fetchRiotAccounts } from '../api/riotAccount'
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
  const accounts = useApi(fetchRiotAccounts)
  // null = the main account
  const [account, setAccount] = useState<string | null>(null)
  const stats = useApi(fetchMyStats, account)
  const list = accounts.status === 'ready' ? accounts.data.accounts : []
  const tabs = list.length > 1 && (
    <div className="account-tabs" role="tablist">
      {list.map((a) => {
        const selected = account === a.id || (account === null && a.isMain)
        return (
          <button
            key={a.id}
            type="button"
            role="tab"
            aria-selected={selected}
            className={selected ? 'account-tab selected' : 'account-tab'}
            onClick={() => setAccount(a.id)}
          >
            {a.riotId}
          </button>
        )
      })}
    </div>
  )

  if (stats.status === 'loading')
    return (
      <>
        {tabs}
        <p className="muted">Liczę Twoje porażki… Pierwszy raz może to potrwać kilka sekund.</p>
      </>
    )
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

  return (
    <>
      {tabs}
      <PlayerStatsView stats={stats.data} title={<h2 className="page-title">{stats.data.riotId}</h2>} />
    </>
  )
}
