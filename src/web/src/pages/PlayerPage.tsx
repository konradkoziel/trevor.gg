import { Link, useParams } from 'react-router'
import { errorText } from '../api/errorText'
import { fetchPlayerProfile } from '../api/players'
import { useApi } from '../api/useApi'
import { LoginLink } from '../features/account/LoginLink'
import { useCurrentUser } from '../features/account/useCurrentUser'
import { BuildList } from '../features/builds/BuildList'
import { playerPath } from '../features/players/playerLinks'
import { TopChampions } from '../features/players/TopChampions'
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

  if (profile.status === 'loading') return <p className="muted">Szukam haków na {riotId}…</p>
  if (profile.status === 'error')
    return (
      <section className="panel">
        <p className="error">{errorText(profile.error, 'Nie udało się pobrać profilu.')}</p>
      </section>
    )

  const p = profile.data
  const title = (
    <div className="player-title">
      {p.profileIconUrl && <img src={p.profileIconUrl} alt="" width={72} height={72} />}
      <div>
        <h2 className="page-title">{p.riotId}</h2>
        <span className="muted">poziom {p.level}</span>
        {p.otherAccounts && p.otherAccounts.length > 0 && (
          <span className="other-accounts muted">
            Inne konta:{' '}
            {p.otherAccounts.map((id, i) => (
              <span key={id}>
                {i > 0 && ', '}
                <PlayerLink riotId={id} />
              </span>
            ))}
          </span>
        )}
      </div>
    </div>
  )
  const mains = (
    <section className="side-section">
      <h3 className="side-title">Najwięcej zmarnowanych godzin</h3>
      <TopChampions champions={p.topChampions} />
    </section>
  )

  return (
    <>
      <PlayerStatsView stats={p.stats} title={title} aside={mains} />
      <section className="home-section">
        <h2 className="page-title">Buildy</h2>
        {p.builds.length > 0 ? (
          <BuildList builds={p.builds} />
        ) : (
          <p className="muted">Żadnych buildów. Gra na czuja, i to widać.</p>
        )}
      </section>
    </>
  )
}

function PlayerLink({ riotId }: { riotId: string }) {
  const path = playerPath(riotId)
  return path ? <Link to={path}>{riotId}</Link> : <>{riotId}</>
}
