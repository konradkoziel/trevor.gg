import type { RankedStanding } from '../../api/ranks'

// Emblem and "Gold II" in one line, for lists like the leaderboard
export function RankBadge({ rank }: { rank: RankedStanding | null | undefined }) {
  if (!rank) return <span className="rank-badge unranked">Unranked</span>

  return (
    <span
      className={`rank-badge tier-${rank.tier.toLowerCase()}`}
      title={`${rank.queue}: ${rank.label}, ${rank.leaguePoints} LP`}
    >
      <span className="rank-badge-emblem">
        <img src={rank.emblemUrl} alt="" loading="lazy" />
      </span>
      <strong>{rank.label}</strong>
    </span>
  )
}
