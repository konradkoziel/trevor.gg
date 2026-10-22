import { Link } from 'react-router'
import type { MasteryChampion } from '../../api/players'
import { newBuildPath } from '../builds/buildLinks'
import { formatWhole } from '../stats/format'

// Mastery mains in the side column; each one opens a new build for that champion
export function TopChampions({ champions }: { champions: MasteryChampion[] }) {
  if (champions.length === 0) return <p className="muted">Zero maestrii. Albo nowe konto, albo smurf.</p>

  return (
    <ol className="mini-list">
      {champions.map((c) => (
        <li key={c.championId}>
          <Link to={newBuildPath(c.championId)} className="mini-link" title={`Zrób build na ${c.name}`}>
            {c.iconUrl && <img src={c.iconUrl} alt="" width={36} height={36} />}
            <span className="mini-main">
              <strong>{c.name}</strong>
              <span className="muted">poziom maestrii {c.level}</span>
            </span>
            <span className="mini-numbers">
              <strong>{formatWhole(c.points)}</strong>
              <span className="muted">pkt</span>
            </span>
          </Link>
        </li>
      ))}
    </ol>
  )
}
