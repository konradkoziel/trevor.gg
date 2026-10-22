import { useState, type ReactNode } from 'react'
import { FlexQueue, SoloQueue } from '../../api/ranks'
import type { PlayerStats } from '../../api/stats'
import { RankCard } from '../ranks/RankCard'
import { ChampionList } from './ChampionList'
import { formatAgo, formatDecimal, formatPercent, winRateTone } from './format'
import { RecentMatches } from './RecentMatches'

interface Props {
  stats: PlayerStats
  // Name line: just the Riot ID on "Statystyki", icon and level on a profile
  title: ReactNode
  // Extra side-column sections, e.g. a profile's mastery mains
  aside?: ReactNode
}

// Ranks and champions on the side, the recent games as the main column (between the two on phones); shared by "Statystyki" and the player profiles
export function PlayerStatsView({ stats: s, title, aside }: Props) {
  // Fixed for the render, so "2 h temu" doesn't change between renders
  const [now] = useState(() => new Date())
  const ranks = s.ranks ?? []
  const solo = ranks.find((r) => r.queue === SoloQueue)
  const flex = ranks.find((r) => r.queue === FlexQueue)
  const lastGame = s.recent[0]

  return (
    <>
      <header className="player-header">
        {title}
        {lastGame && <span className="muted">ostatnia gra {formatAgo(new Date(lastGame.playedAt), now)}</span>}
      </header>

      <div className="player-layout">
        <div className="player-ranks">
          <RankCard queue={SoloQueue} rank={solo} primary={!!solo || !flex} />
          <RankCard queue={FlexQueue} rank={flex} primary={!solo && !!flex} />
        </div>

        <aside className="player-aside">
          {s.champions.length > 0 && (
            <section className="side-section">
              <h3 className="side-title">Championy · ostatnie gry</h3>
              <ChampionList champions={s.champions} />
            </section>
          )}
          {aside}
        </aside>

        <section className="player-main">
          <div className="recent-summary">
            <h3 className="side-title">Ostatnie {s.recent.length} gier</h3>
            <span>
              {s.wins}W {s.games - s.wins}L
            </span>
            <strong className={winRateTone(s.winRate)}>{formatPercent(s.winRate)}</strong>
            <span>{formatDecimal(s.kda)} KDA</span>
          </div>
          {s.recent.length > 0 ? (
            <RecentMatches matches={s.recent} riotId={s.riotId} />
          ) : (
            <p className="muted">Brak gier. Może czas zagrać?</p>
          )}
          <p className="muted small">Mecze z Riot API, bez remake'ów. Bilans, winrate i KDA tylko z Summoner's Rift.</p>
        </section>
      </div>
    </>
  )
}
