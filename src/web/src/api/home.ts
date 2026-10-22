import type { Champion } from './champions'
import { cachedOnce, getJson } from './client'
import type { RankedStanding } from './ranks'

// Mirror Web.Api.Features.Home.LeaderboardEntry and Web.Api.Features.Builds.RecentBuild
export interface LeaderboardEntry {
  riotId: string
  profileIconUrl: string | null
  level: number
  masteryPoints: number
  topChampionName: string | null
  topChampionIconUrl: string | null
  topChampionPoints: number
  rank: RankedStanding | null
  // How many linked accounts the mastery points add up
  accountCount: number
}

export interface RecentBuild {
  id: string
  name: string
  championId: string
  ownerName: string
  updatedAt: string
}

export const fetchRotation = cachedOnce(() => getJson<Champion[]>('rotation'))

export const fetchLeaderboard = (signal: AbortSignal) => getJson<LeaderboardEntry[]>('leaderboard', signal)

export const fetchRecentBuilds = (signal: AbortSignal) => getJson<RecentBuild[]>('recent-builds', signal)
