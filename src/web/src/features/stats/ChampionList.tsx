import type { ChampionStats } from '../../api/stats'
import { formatDecimal, formatPercent, winRateTone } from './format'

// Champions of the recent games, one compact row each, for the side column
export function ChampionList({ champions }: { champions: ChampionStats[] }) {
  return (
    <ul className="mini-list">
      {champions.map((c) => (
        <li
          key={c.championName}
          title={`${formatDecimal(c.csPerMinute)} CS/min · ${Math.round(c.damagePerMinute)} DMG/min`}
        >
          {c.iconUrl && <img src={c.iconUrl} alt="" width={36} height={36} />}
          <span className="mini-main">
            <strong>{c.championName}</strong>
            <span className="muted">
              {c.games} {c.games === 1 ? 'gra' : 'gier'}
            </span>
          </span>
          <span className="mini-numbers">
            <strong className={winRateTone(c.winRate)}>{formatPercent(c.winRate)}</strong>
            <span className="muted">{formatDecimal(c.kda)} KDA</span>
          </span>
        </li>
      ))}
    </ul>
  )
}
