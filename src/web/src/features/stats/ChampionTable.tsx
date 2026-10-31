import { Link } from 'react-router'
import type { MasteryChampion } from '../../api/players'
import type { ChampionStats } from '../../api/stats'
import { championBuildsPath } from '../builds/buildLinks'
import { championRows } from './championRows'
import { formatDecimal, formatPercent, formatWhole, plural, winRateTone } from './format'

interface Props {
  played: ChampionStats[]
  // A profile's mastery mains; your own stats have none
  masteries?: MasteryChampion[]
}

// Recent games per champion next to mastery points; a champion opens the team's builds of it
export function ChampionTable({ played, masteries }: Props) {
  const rows = championRows(played, masteries)
  if (rows.length === 0) return <p className="muted">Ani jednego championa. Ktoś tu w ogóle gra?</p>
  const withMastery = !!masteries

  return (
    <div className="table-scroll">
      <table className="champion-table">
        <thead>
          <tr>
            <th scope="col">Champion</th>
            <th scope="col">Gry</th>
            <th scope="col">Winrate</th>
            <th scope="col">KDA</th>
            <th scope="col">CS/min</th>
            {withMastery && <th scope="col">Maestria</th>}
          </tr>
        </thead>
        <tbody>
          {rows.map((r) => (
            <tr key={r.id}>
              <th scope="row">
                <Link to={championBuildsPath(r.id)} className="champion-cell" title={`Buildy ekipy na ${r.name}`}>
                  {r.iconUrl && <img src={r.iconUrl} alt="" width={32} height={32} loading="lazy" />}
                  <span>{r.name}</span>
                </Link>
              </th>
              <td>{r.games > 0 ? `${r.games} ${plural(r.games, 'gra', 'gry', 'gier')}` : '—'}</td>
              <td>
                {r.winRate === null ? (
                  '—'
                ) : (
                  <strong className={winRateTone(r.winRate)}>{formatPercent(r.winRate)}</strong>
                )}
              </td>
              <td>{r.kda === null ? '—' : formatDecimal(r.kda)}</td>
              <td>{r.csPerMinute === null ? '—' : formatDecimal(r.csPerMinute)}</td>
              {withMastery && <td>{r.masteryPoints === null ? '—' : formatWhole(r.masteryPoints)}</td>}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
