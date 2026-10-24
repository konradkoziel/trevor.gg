import { useMemo } from 'react'
import { Link } from 'react-router'
import { fetchChampions } from '../../api/champions'
import type { RecentBuild } from '../../api/home'
import { useApi } from '../../api/useApi'
import { buildPath } from './buildLinks'

const dateFormat = new Intl.DateTimeFormat('pl-PL', { day: 'numeric', month: 'short' })

// Builds of the whole team: champion icon, name, whose and when; "Najnowsze buildy ekipy" and a champion's builds
export function TeamBuildList({ builds }: { builds: RecentBuild[] }) {
  const champions = useApi(fetchChampions)
  const iconById = useMemo(
    () => new Map(champions.status === 'ready' ? champions.data.map((c) => [c.id, c.iconUrl]) : []),
    [champions],
  )

  return (
    <ul className="my-builds">
      {builds.map((b) => (
        <li key={b.id}>
          <Link to={buildPath(b.id)} className="my-build">
            {iconById.has(b.championId) && <img src={iconById.get(b.championId)} alt="" width={40} height={40} />}
            <span className="my-build-name">{b.name}</span>
            <span className="muted">
              {b.ownerName} · {dateFormat.format(new Date(b.updatedAt))}
            </span>
          </Link>
        </li>
      ))}
    </ul>
  )
}
