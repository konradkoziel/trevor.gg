import { useMemo } from 'react'
import { Link } from 'react-router'
import { fetchChampions } from '../../api/champions'
import { fetchRecentBuilds } from '../../api/home'
import { useApi } from '../../api/useApi'
import { buildPath } from '../builds/buildLinks'

export function RecentBuilds() {
  const builds = useApi(fetchRecentBuilds)
  const champions = useApi(fetchChampions)
  const iconById = useMemo(
    () => new Map(champions.status === 'ready' ? champions.data.map((c) => [c.id, c.iconUrl]) : []),
    [champions],
  )

  if (builds.status !== 'ready' || builds.data.length === 0) return null

  return (
    <section className="home-section">
      <h2 className="page-title">Najnowsze buildy ekipy</h2>
      <ul className="my-builds">
        {builds.data.map((b) => (
          <li key={b.id}>
            <Link to={buildPath(b.id)} className="my-build">
              {iconById.has(b.championId) && <img src={iconById.get(b.championId)} alt="" width={40} height={40} />}
              <span className="my-build-name">{b.name}</span>
              <span className="muted">{b.ownerName}</span>
            </Link>
          </li>
        ))}
      </ul>
    </section>
  )
}
