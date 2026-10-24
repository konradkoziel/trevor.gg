import { Link, useParams } from 'react-router'
import { fetchChampionBuilds } from '../api/builds'
import { fetchChampions, type Champion } from '../api/champions'
import { useApi } from '../api/useApi'
import { LoginLink } from '../features/account/LoginLink'
import { hasSession, useCurrentUser } from '../features/account/useCurrentUser'
import { buildsPath, newBuildPath } from '../features/builds/buildLinks'
import { TeamBuildList } from '../features/builds/TeamBuildList'
import { Skeleton } from '../features/layout/Skeleton'

// /buildy/:championId: the team's builds of one champion and the button to make a new one
export function ChampionBuildsPage() {
  const { championId = '' } = useParams()
  const champions = useApi(fetchChampions)

  if (champions.status === 'loading') return <Skeleton rows={1} height={96} />
  if (champions.status === 'error') return <p className="error">Nie udało się pobrać championów.</p>

  const champion = champions.data.find((c) => c.id === championId)
  if (!champion)
    return (
      <p className="error">
        Nie ma takiego championa. <Link to={buildsPath}>Wybierz z listy</Link>.
      </p>
    )

  return (
    <>
      <header className="champion-head">
        <img src={champion.iconUrl} alt="" width={72} height={72} />
        <div>
          <h1 className="champion-head-name">{champion.name}</h1>
          <span className="muted">{champion.title}</span>
        </div>
        <div className="champion-head-actions">
          <Link className="button gold" to={newBuildPath(champion.id)}>
            Nowy build
          </Link>
          <Link className="button secondary" to={buildsPath}>
            Inny champion
          </Link>
        </div>
      </header>
      <TeamBuilds champion={champion} />
    </>
  )
}

function TeamBuilds({ champion }: { champion: Champion }) {
  const { state } = useCurrentUser()
  if (!hasSession(state))
    return (
      <section className="panel">
        <p>Buildy ekipy widać po zalogowaniu. Nowy możesz zacząć i bez tego, zapisać już nie.</p>
        <LoginLink />
      </section>
    )
  return <Builds champion={champion} />
}

function Builds({ champion }: { champion: Champion }) {
  const builds = useApi(fetchChampionBuilds, champion.id)

  return (
    <section className="home-section">
      <h2 className="page-title">Buildy ekipy</h2>
      {builds.status === 'loading' && <Skeleton rows={3} />}
      {builds.status === 'error' && <p className="error">Nie udało się pobrać buildów.</p>}
      {builds.status === 'ready' &&
        (builds.data.length === 0 ? (
          <p className="muted">Nikt jeszcze nie zrobił buildu na {champion.name}. Bądź pierwszy, niech inni kopiują.</p>
        ) : (
          <TeamBuildList builds={builds.data} />
        ))}
    </section>
  )
}
