import { Link } from 'react-router'
import type { MasteryChampion } from '../../api/players'
import { newBuildPath } from '../builds/buildLinks'
import { formatWhole } from '../stats/format'

export function TopChampions({ champions }: { champions: MasteryChampion[] }) {
  if (champions.length === 0) return <p className="muted">Zero maestrii. Albo nowe konto, albo smurf.</p>

  return (
    <ol className="top-champions">
      {champions.map((c) => (
        <li key={c.championId}>
          <Link to={newBuildPath(c.championId)} className="top-champion" title={`Zrób build na ${c.name}`}>
            {c.iconUrl && <img src={c.iconUrl} alt="" width={64} height={64} />}
            <strong>{c.name}</strong>
            <span className="muted">
              {formatWhole(c.points)} pkt · M{c.level}
            </span>
          </Link>
        </li>
      ))}
    </ol>
  )
}
