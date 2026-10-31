import { Link, useParams } from 'react-router'
import { errorText } from '../api/errorText'
import { fetchPlayerProfile } from '../api/players'
import { useApi } from '../api/useApi'
import { LoginLink } from '../features/account/LoginLink'
import { useCurrentUser } from '../features/account/useCurrentUser'
import { playerPath } from '../features/players/playerLinks'
import { Skeleton } from '../features/layout/Skeleton'
import { PlayerStatsView } from '../features/stats/PlayerStatsView'

// /gracz/:name/:tag: a linked friend's rank, mains, games and builds
export function PlayerPage() {
  const { name = '', tag = '' } = useParams()
  const { state } = useCurrentUser()

  if (state.status === 'anonymous')
    return (
      <section className="panel">
        <p>Profile graczy są dla zalogowanych. Obcym Trevor nie pokazuje cudzych porażek.</p>
        <LoginLink />
      </section>
    )
  return <Profile riotId={`${name}#${tag}`} />
}

function Profile({ riotId }: { riotId: string }) {
  const profile = useApi(fetchPlayerProfile, riotId)

  if (profile.status === 'loading')
    return (
      <>
        <p className="muted">Szukam haków na {riotId}…</p>
        <Skeleton rows={1} height={220} />
      </>
    )
  if (profile.status === 'error')
    return (
      <section className="panel">
        <p className="error">{errorText(profile.error, 'Nie udało się pobrać profilu.')}</p>
      </section>
    )

  const p = profile.data
  const otherAccounts = p.otherAccounts && p.otherAccounts.length > 0 && (
    <span className="other-accounts muted">
      Inne konta:{' '}
      {p.otherAccounts.map((id, i) => (
        <span key={id}>
          {i > 0 && ', '}
          <PlayerLink riotId={id} />
        </span>
      ))}
    </span>
  )

  return (
    <PlayerStatsView
      stats={p.stats}
      iconUrl={p.profileIconUrl}
      level={p.level}
      subtitle={otherAccounts}
      masteries={p.topChampions}
      builds={p.builds}
    />
  )
}

function PlayerLink({ riotId }: { riotId: string }) {
  const path = playerPath(riotId)
  return path ? <Link to={path}>{riotId}</Link> : <>{riotId}</>
}
