import { getJson } from './client'

// Mirror Web.Api.Features.Matches (MatchDetail, MatchTeam, MatchPlayer, MatchIcon)
export interface MatchIcon {
  name: string
  iconUrl: string | null
}

export interface MatchPlayer {
  riotId: string | null
  championName: string
  championIconUrl: string | null
  championLevel: number
  kills: number
  deaths: number
  assists: number
  creepScore: number
  damage: number
  gold: number
  visionScore: number
  items: (MatchIcon | null)[]
  spells: MatchIcon[]
  keystone: MatchIcon | null
  secondaryTree: MatchIcon | null
}

export interface MatchTeam {
  teamId: number
  win: boolean
  kills: number
  deaths: number
  assists: number
  gold: number
  players: MatchPlayer[]
}

export interface MatchDetail {
  matchId: string
  queue: string
  durationSeconds: number
  playedAt: string
  teams: MatchTeam[]
}

export const fetchMatch = (signal: AbortSignal, matchId: string) =>
  getJson<MatchDetail>(`matches/${encodeURIComponent(matchId)}`, signal)
