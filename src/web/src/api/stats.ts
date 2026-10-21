import { getJson } from './client'

// Mirror Web.Api.Features.Stats (PlayerStats, ChampionStats, RecentMatch)
export interface ChampionStats {
  championName: string
  iconUrl: string | null
  games: number
  wins: number
  winRate: number
  kda: number
  csPerMinute: number
  damagePerMinute: number
}

export interface RecentMatch {
  championName: string
  iconUrl: string | null
  win: boolean
  kills: number
  deaths: number
  assists: number
  csPerMinute: number
  queue: string
  playedAt: string
}

export interface PlayerStats {
  riotId: string
  games: number
  wins: number
  winRate: number
  kda: number
  champions: ChampionStats[]
  recent: RecentMatch[]
}

export const fetchMyStats = (signal: AbortSignal) => getJson<PlayerStats>('me/stats', signal)
