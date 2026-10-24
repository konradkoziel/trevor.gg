import { Link } from 'react-router'
import { ApiRequestError } from '../api/client'
import { errorText } from '../api/errorText'
import { useState } from 'react'
import { fetchRiotAccounts } from '../api/riotAccount'
import { fetchMyStats } from '../api/stats'
import { useApi } from '../api/useApi'
import { myAccountsPath } from '../features/account/accountLinks'
import { Skeleton } from '../features/layout/Skeleton'
import { PlayerStatsView } from '../features/stats/PlayerStatsView'

// /ja (MePage checks the login)
export function StatsPage() {
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
        <Skeleton rows={6} height={56} />
      </>
    )
  if (stats.status === 'error') {
    const notLinked = stats.error instanceof ApiRequestError && stats.error.status === 404
    return (
      <section className="panel">
        <p className="error">{errorText(stats.error, 'Nie udało się pobrać statystyk.')}</p>
        {notLinked && (
          <Link className="button gold" to={myAccountsPath}>
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
