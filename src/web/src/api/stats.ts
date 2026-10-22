import { getJson } from './client'
import type { RankedStanding } from './ranks'

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
  matchId: string
  championName: string
  iconUrl: string | null
  win: boolean
  kills: number
  deaths: number
  assists: number
  csPerMinute: number
  queue: string
  playedAt: string
  durationSeconds: number
}

export interface PlayerStats {
  riotId: string
  games: number
  wins: number
  winRate: number
  kda: number
  champions: ChampionStats[]
  recent: RecentMatch[]
  ranks: RankedStanding[] | null
}

// account: one of the user's accounts by id; null = the main one
export const fetchMyStats = (signal: AbortSignal, account: string | null) =>
  getJson<PlayerStats>(account ? `me/stats?account=${encodeURIComponent(account)}` : 'me/stats', signal)
