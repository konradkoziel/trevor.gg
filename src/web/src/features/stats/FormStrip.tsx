import type { RecentMatch } from '../../api/stats'

// The recent games as W/L squares, newest on the left: a streak or a tilt is visible at a glance
export function FormStrip({ matches }: { matches: RecentMatch[] }) {
  if (matches.length === 0) return null

  return (
    <ol className="form-strip" aria-label="Forma: ostatnie gry, od najnowszej">
      {matches.map((m) => (
        <li
          key={m.matchId}
          className={m.win ? 'win' : 'loss'}
          title={`${m.win ? 'Wygrana' : 'Przegrana'}: ${m.championName} ${m.kills}/${m.deaths}/${m.assists}, ${m.queue}`}
        >
          {m.win ? 'W' : 'L'}
        </li>
      ))}
    </ol>
  )
}
