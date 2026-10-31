import type { RecentMatch } from '../../api/stats'
import { playerBadges } from './badges'

// Trevor's comments on the recent games ("Tilt: 4 porażki z rzędu"); nothing when the games are unremarkable
export function PlayerBadges({ matches }: { matches: RecentMatch[] }) {
  const badges = playerBadges(matches)
  if (badges.length === 0) return null

  return (
    <ul className="player-badges">
      {badges.map((b) => (
        <li key={b.label} className={`player-badge ${b.tone}`} title={b.detail}>
          {b.label}
        </li>
      ))}
    </ul>
  )
}
