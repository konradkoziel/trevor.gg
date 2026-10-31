import { useState, type CSSProperties, type ReactNode } from 'react'
import { fetchChampions } from '../../api/champions'
import { FlexQueue, SoloQueue } from '../../api/ranks'
import type { PlayerStats } from '../../api/stats'
import { useApi } from '../../api/useApi'
import { RankCard } from '../ranks/RankCard'
import { FormStrip } from './FormStrip'
import { formatAgo, formatDecimal, formatPercent, plural, winRateTone } from './format'

interface Props {
  stats: PlayerStats
  iconUrl?: string | null
  level?: number
  subtitle?: ReactNode
  // Data Dragon id or Riot's name of the champion whose splash art is the background
  mainChampion?: string
}

// The top of a profile: splash art of their main, picture, name, the numbers that matter, ranks and recent form
export function PlayerHero({ stats: s, iconUrl, level, subtitle, mainChampion }: Props) {
  // Fixed for the render, so "2 h temu" doesn't change between renders
  const [now] = useState(() => new Date())
  const champions = useApi(fetchChampions)
  const main =
    champions.status === 'ready' && mainChampion
      ? champions.data.find((c) => c.id === mainChampion || c.name === mainChampion)
      : undefined
  const ranks = s.ranks ?? []
  const solo = ranks.find((r) => r.queue === SoloQueue)
  const flex = ranks.find((r) => r.queue === FlexQueue)
  const lastGame = s.recent[0]
  const splash = main?.splashUrl ? ({ '--splash': `url(${main.splashUrl})` } as CSSProperties) : undefined

  return (
    <section className={splash ? 'player-hero with-splash' : 'player-hero'} style={splash}>
      <div className="player-hero-who">
        {iconUrl && (
          <div className="player-avatar">
            <img src={iconUrl} alt="" width={88} height={88} />
            {level !== undefined && <span className="player-level">{level}</span>}
          </div>
        )}
        <div className="player-hero-text">
          <h1 className="player-name">{s.riotId}</h1>
          {subtitle}
          <ul className="player-chips">
            {s.games > 0 && (
              <li>
                <strong className={winRateTone(s.winRate)}>{formatPercent(s.winRate)}</strong> wygranych w {s.games}{' '}
                {plural(s.games, 'grze', 'grach', 'grach')}
              </li>
            )}
            {s.games > 0 && (
              <li>
                <strong>{formatDecimal(s.kda)}</strong> KDA
              </li>
            )}
            {main && (
              <li>
                main: <strong>{main.name}</strong>
              </li>
            )}
            {lastGame && <li>ostatnia gra {formatAgo(new Date(lastGame.playedAt), now)}</li>}
          </ul>
        </div>
      </div>

      <div className="player-hero-ranks">
        <RankCard queue={SoloQueue} rank={solo} primary={!!solo || !flex} />
        <RankCard queue={FlexQueue} rank={flex} primary={!solo && !!flex} />
      </div>

      <FormStrip matches={s.recent} />
    </section>
  )
}
