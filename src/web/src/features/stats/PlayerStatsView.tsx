import { useState, type ReactNode } from 'react'
import type { BuildSummary } from '../../api/builds'
import type { MasteryChampion } from '../../api/players'
import type { PlayerStats } from '../../api/stats'
import { BuildList } from '../builds/BuildList'
import { ChampionTable } from './ChampionTable'
import { PlayerBadges } from './PlayerBadges'
import { PlayerHero } from './PlayerHero'
import { RecentMatches } from './RecentMatches'

interface Props {
  stats: PlayerStats
  // Profile picture and account level; only profiles have them
  iconUrl?: string | null
  level?: number
  // Under the name, e.g. a profile's other accounts
  subtitle?: ReactNode
  // A profile's mastery mains, joined into the champion table
  masteries?: MasteryChampion[]
  // A profile's builds get their own tab; your own are under Ja → Buildy
  builds?: BuildSummary[]
}

type Tab = 'matches' | 'champions' | 'builds'

// Games shown before "Pokaż więcej": a screen's worth on a laptop
const FirstMatches = 10

// Banner with ranks and form, Trevor's badges, then the games, champions and builds as tabs; shared by Ja → Statystyki and the profiles
export function PlayerStatsView({ stats: s, iconUrl, level, subtitle, masteries, builds }: Props) {
  const [tab, setTab] = useState<Tab>('matches')
  const [allMatches, setAllMatches] = useState(false)
  const mainChampion = masteries?.[0]?.championId ?? [...s.champions].sort((a, b) => b.games - a.games)[0]?.championName
  const tabs: [Tab, string][] = [
    ['matches', `Mecze (${s.recent.length})`],
    ['champions', 'Championy'],
    ...(builds ? [['builds', `Buildy (${builds.length})`] as [Tab, string]] : []),
  ]
  const shown = allMatches ? s.recent : s.recent.slice(0, FirstMatches)

  return (
    <>
      <PlayerHero stats={s} iconUrl={iconUrl} level={level} subtitle={subtitle} mainChampion={mainChampion} />
      <PlayerBadges matches={s.recent} />

      <div className="page-tabs" role="tablist">
        {tabs.map(([id, label]) => (
          <button
            key={id}
            type="button"
            role="tab"
            aria-selected={tab === id}
            className={tab === id ? 'active' : undefined}
            onClick={() => setTab(id)}
          >
            {label}
          </button>
        ))}
      </div>

      {tab === 'matches' && (
        <section>
          {s.recent.length > 0 ? (
            <RecentMatches matches={shown} riotId={s.riotId} />
          ) : (
            <p className="muted">Brak gier. Może czas zagrać?</p>
          )}
          {shown.length < s.recent.length && (
            <button type="button" className="button secondary show-more" onClick={() => setAllMatches(true)}>
              Pokaż wszystkie ({s.recent.length})
            </button>
          )}
          <p className="muted small">
            Mecze z Riot API, bez remake'ów. Winrate i KDA w banerze tylko z Summoner's Rift.
          </p>
        </section>
      )}
      {tab === 'champions' && <ChampionTable played={s.champions} masteries={masteries} />}
      {tab === 'builds' &&
        builds &&
        (builds.length > 0 ? (
          <BuildList builds={builds} />
        ) : (
          <p className="muted">Żadnych buildów. Gra na czuja, i to widać.</p>
        ))}
    </>
  )
}
