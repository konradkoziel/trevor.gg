import type { MasteryChampion } from '../../api/players'
import type { ChampionStats } from '../../api/stats'

// One champion in the "Championy" tab: recent games and, on a profile, mastery; null where there is none
export interface ChampionRow {
  // For the champion's builds page: Data Dragon id when known, else Riot's name from the match
  id: string
  name: string
  iconUrl: string | null
  games: number
  winRate: number | null
  kda: number | null
  csPerMinute: number | null
  masteryPoints: number | null
}

// The champions of the recent games first (most played first), then mastery mains not played lately.
// Riot's match names a champion by its id (MonkeyKing), mastery comes with the Polish name: either matches
export function championRows(played: ChampionStats[], masteries: MasteryChampion[] = []): ChampionRow[] {
  const masteryOf = (name: string) => masteries.find((m) => m.championId === name || m.name === name)
  const rows: ChampionRow[] = [...played]
    .sort((a, b) => b.games - a.games)
    .map((c) => {
      const mastery = masteryOf(c.championName)
      return {
        id: mastery?.championId ?? c.championName,
        name: mastery?.name ?? c.championName,
        iconUrl: c.iconUrl ?? mastery?.iconUrl ?? null,
        games: c.games,
        winRate: c.winRate,
        kda: c.kda,
        csPerMinute: c.csPerMinute,
        masteryPoints: mastery?.points ?? null,
      }
    })
  const notPlayed = masteries
    .filter((m) => !rows.some((r) => r.id === m.championId))
    .sort((a, b) => b.points - a.points)
    .map((m) => ({
      id: m.championId,
      name: m.name,
      iconUrl: m.iconUrl,
      games: 0,
      winRate: null,
      kda: null,
      csPerMinute: null,
      masteryPoints: m.points,
    }))
  return [...rows, ...notPlayed]
}
