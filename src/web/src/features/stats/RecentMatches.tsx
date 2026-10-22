import { useState } from 'react'
import type { RecentMatch } from '../../api/stats'
import { formatDecimal, formatDuration, kdaRatio } from './format'
import { MatchDetails } from './MatchDetails'

const dateFormat = new Intl.DateTimeFormat('pl-PL', {
  day: 'numeric',
  month: 'short',
  hour: '2-digit',
  minute: '2-digit',
})

// One line per game: result colour, champion, score with its KDA ratio, CS and length; a click opens both teams
export function RecentMatches({ matches, riotId }: { matches: RecentMatch[]; riotId: string }) {
  const [open, setOpen] = useState<string>()

  return (
    <ul className="recent-matches">
      {matches.map((m) => {
        const expanded = open === m.matchId
        return (
          <li key={m.matchId} className={m.win ? 'recent-item win' : 'recent-item loss'}>
            <button
              type="button"
              className={m.win ? 'recent win' : 'recent loss'}
              onClick={() => setOpen(expanded ? undefined : m.matchId)}
              aria-expanded={expanded}
            >
              {m.iconUrl && <img src={m.iconUrl} alt="" width={40} height={40} />}
              <span className="recent-main">
                <strong>{m.championName}</strong>
                <span className="muted">
                  {m.win ? 'Wygrana' : 'Przegrana'} · {m.queue}
                </span>
              </span>
              <span className="recent-score">
                <strong>
                  {m.kills} / <span className="deaths">{m.deaths}</span> / {m.assists}
                </strong>
                <span className="muted">{formatDecimal(kdaRatio(m.kills, m.deaths, m.assists))} KDA</span>
              </span>
              <span className="recent-extra muted">
                <span>{formatDecimal(m.csPerMinute)} CS/min</span>
                <span>
                  {formatDuration(m.durationSeconds)} · {dateFormat.format(new Date(m.playedAt))}
                </span>
              </span>
              <span className="recent-toggle" aria-hidden="true">
                {expanded ? '▴' : '▾'}
              </span>
            </button>
            {expanded && <MatchDetails matchId={m.matchId} riotId={riotId} />}
          </li>
        )
      })}
    </ul>
  )
}
