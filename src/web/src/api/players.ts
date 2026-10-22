import type { BuildSummary } from './builds'
import { getJson } from './client'
import type { PlayerStats } from './stats'
import { splitRiotId } from '../features/players/playerLinks'

// Mirror Web.Api.Features.Players (PlayerProfile, MasteryChampion)
export interface MasteryChampion {
  championId: string
  name: string
  iconUrl: string | null
  level: number
  points: number
}

export interface PlayerProfile {
  riotId: string
  profileIconUrl: string | null
  level: number
  topChampions: MasteryChampion[]
  stats: PlayerStats
  builds: BuildSummary[]
}

// Takes the route's name and tag joined as "Name#TAG", so useApi has one string to watch
export function fetchPlayerProfile(signal: AbortSignal, riotId: string) {
  const id = splitRiotId(riotId)
  if (!id) return Promise.reject(new Error(`Not a Riot ID: ${riotId}`))
  return getJson<PlayerProfile>(`players/${encodeURIComponent(id.name)}/${encodeURIComponent(id.tag)}`, signal)
}
