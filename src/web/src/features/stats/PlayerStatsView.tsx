import type { ReactNode } from 'react'
import type { PlayerStats } from '../../api/stats'
import { RankBadge } from '../ranks/RankBadge'
import { ChampionTable } from './ChampionTable'
import { formatDecimal, formatPercent, winRateTone } from './format'
import { RecentMatches } from './RecentMatches'

// Ranks, summary tiles, champions and recent games; shared by "Statystyki" and the player profiles
export function PlayerStatsView({ stats: s, header }: { stats: PlayerStats; header: ReactNode }) {
  return (
    <>
      <section className="stats-summary">
        {header}
        <div className="rank-row">
          {s.ranks && s.ranks.length > 0 ? (
            s.ranks.map((r) => <RankBadge key={r.queue} rank={r} />)
          ) : (
            <RankBadge rank={null} />
          )}
        </div>
        <div className="summary-tiles">
          <div className="tile">
            <span className="tile-value">{s.games}</span>
            <span className="muted">gier na Rifcie</span>
          </div>
          <div className="tile">
            <span className={`tile-value ${winRateTone(s.winRate)}`}>{formatPercent(s.winRate)}</span>
            <span className="muted">
              winrate ({s.wins}W / {s.games - s.wins}L)
            </span>
          </div>
          <div className="tile">
            <span className="tile-value">{formatDecimal(s.kda)}</span>
            <span className="muted">KDA</span>
          </div>
        </div>
        <p className="muted">
          Ostatnie mecze z Riot API, bez remake'ów. Statystyki championów tylko z Summoner's Rift.
        </p>
      </section>

      {s.champions.length > 0 && (
        <section className="home-section">
          <h2 className="page-title">Championy</h2>
          <ChampionTable champions={s.champions} />
        </section>
      )}
      <section className="home-section">
        <h2 className="page-title">Ostatnie gry</h2>
        {s.recent.length > 0 ? (
          <RecentMatches matches={s.recent} />
        ) : (
          <p className="muted">Brak gier. Może czas zagrać?</p>
        )}
      </section>
    </>
  )
}
