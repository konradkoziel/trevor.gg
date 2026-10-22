import type { RankedStanding } from '../../api/ranks'
import { WinRateRing } from '../stats/WinRateRing'
import { winRateOf } from '../stats/format'
import { divisionProgress } from './rankProgress'

interface Props {
  queue: string
  rank: RankedStanding | undefined
  // The main queue gets the big emblem, the other one a slim card
  primary?: boolean
}

// One ranked queue: emblem, division, LP towards the next one and this season's record from Riot
export function RankCard({ queue, rank, primary = false }: Props) {
  const size = primary ? 'primary' : 'secondary'
  if (!rank)
    return (
      <section className={`rank-card ${size} unranked`}>
        <span className="rank-queue">{queue}</span>
        <strong>Unranked</strong>
      </section>
    )

  const progress = divisionProgress(rank)
  return (
    <section className={`rank-card ${size} tier-${rank.tier.toLowerCase()}`}>
      <span className="rank-queue">{queue}</span>
      <div className="rank-card-body">
        <div className="rank-emblem">
          <img src={rank.emblemUrl} alt="" loading="lazy" />
        </div>
        <div className="rank-card-text">
          <strong className="rank-label">{rank.label}</strong>
          <span>{rank.leaguePoints} LP</span>
          {progress !== null && (
            <span className="lp-bar" aria-hidden="true">
              <span style={{ width: `${progress * 100}%` }} />
            </span>
          )}
          <span className="muted">
            Sezon: {rank.wins}W {rank.losses}L
          </span>
        </div>
        {primary && <WinRateRing winRate={winRateOf(rank.wins, rank.losses)} />}
      </div>
    </section>
  )
}
