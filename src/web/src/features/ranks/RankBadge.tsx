import type { RankedStanding } from '../../api/ranks'

interface Props {
  rank: RankedStanding | null | undefined
  // compact: emblem and "Gold II" only, for lists
  compact?: boolean
}

export function RankBadge({ rank, compact = false }: Props) {
  if (!rank) return <span className="rank-badge unranked">Unranked</span>

  const games = rank.wins + rank.losses
  return (
    <span
      className={`rank-badge tier-${rank.tier.toLowerCase()}`}
      title={`${rank.queue}: ${rank.label}, ${rank.leaguePoints} LP`}
    >
      <img src={rank.emblemUrl} alt="" width={compact ? 28 : 56} height={compact ? 28 : 56} loading="lazy" />
      <span className="rank-text">
        <strong>{rank.label}</strong>
        {!compact && (
          <span className="muted">
            {rank.queue} · {rank.leaguePoints} LP · {rank.wins}W/{rank.losses}L
            {games > 0 && ` (${Math.round((100 * rank.wins) / games)}%)`}
          </span>
        )}
      </span>
    </span>
  )
}
