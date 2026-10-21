import type { ChampionStats } from '../../api/stats'
import { formatDecimal, formatPercent, formatWhole, winRateTone } from './format'

export function ChampionTable({ champions }: { champions: ChampionStats[] }) {
  return (
    <div className="table-scroll">
      <table className="stats-table">
        <thead>
          <tr>
            <th>Champion</th>
            <th>Gry</th>
            <th>Winrate</th>
            <th>KDA</th>
            <th>CS/min</th>
            <th>DMG/min</th>
          </tr>
        </thead>
        <tbody>
          {champions.map((c) => (
            <tr key={c.championName}>
              <td>
                <span className="champion-cell">
                  {c.iconUrl && <img src={c.iconUrl} alt="" width={32} height={32} />}
                  {c.championName}
                </span>
              </td>
              <td>{c.games}</td>
              <td className={winRateTone(c.winRate)}>{formatPercent(c.winRate)}</td>
              <td>{formatDecimal(c.kda)}</td>
              <td>{formatDecimal(c.csPerMinute)}</td>
              <td>{formatWhole(c.damagePerMinute)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
