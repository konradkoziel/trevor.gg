import { useMemo } from 'react'
import { Link } from 'react-router'
import { fetchMyBuilds } from '../../api/builds'
import { fetchChampions } from '../../api/champions'
import { useApi } from '../../api/useApi'
import { buildPath } from './buildLinks'

const dateFormat = new Intl.DateTimeFormat('pl-PL', { day: 'numeric', month: 'short', year: 'numeric' })

export function MyBuilds() {
  const builds = useApi(fetchMyBuilds)
  const champions = useApi(fetchChampions)
  const iconById = useMemo(
    () => new Map(champions.status === 'ready' ? champions.data.map((c) => [c.id, c.iconUrl]) : []),
    [champions],
  )

  if (builds.status === 'loading') return <p className="muted">Ładuję Twoje buildy…</p>
  if (builds.status === 'error') return <p className="error">Nie udało się pobrać Twoich buildów.</p>
  if (builds.data.length === 0) return <p className="muted">Nie masz jeszcze żadnego buildu. Wybierz championa poniżej.</p>

  return (
    <ul className="my-builds">
      {builds.data.map((b) => (
        <li key={b.id}>
          <Link to={buildPath(b.id)} className="my-build">
            {iconById.has(b.championId) && <img src={iconById.get(b.championId)} alt="" width={40} height={40} />}
            <span className="my-build-name">{b.name}</span>
            <span className="muted">{dateFormat.format(new Date(b.updatedAt))}</span>
          </Link>
        </li>
      ))}
    </ul>
  )
}
