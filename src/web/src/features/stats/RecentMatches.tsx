import type { RecentMatch } from '../../api/stats'
import { formatDecimal } from './format'

const dateFormat = new Intl.DateTimeFormat('pl-PL', {
  day: 'numeric',
  month: 'short',
  hour: '2-digit',
  minute: '2-digit',
})

export function RecentMatches({ matches }: { matches: RecentMatch[] }) {
  return (
    <ul className="recent-matches">
      {matches.map((m) => (
        <li key={m.playedAt} className={m.win ? 'recent win' : 'recent loss'}>
          {m.iconUrl && <img src={m.iconUrl} alt="" width={36} height={36} />}
          <div className="recent-main">
            <strong>{m.win ? 'Wygrana' : 'Przegrana'}</strong> · {m.championName}
            <span className="muted">
              {' '}
              · {m.queue} · {dateFormat.format(new Date(m.playedAt))}
            </span>
          </div>
          <div className="recent-numbers">
            <span>
              {m.kills}/{m.deaths}/{m.assists}
            </span>
            <span className="muted">{formatDecimal(m.csPerMinute)} CS/min</span>
          </div>
        </li>
      ))}
    </ul>
  )
}
