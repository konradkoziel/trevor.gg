import type { Build } from '../features/builds/build'
import { getJson, sendJson } from './client'
import type { RecentBuild } from './home'

// Mirror Web.Api.Features.Builds (BuildResponse, BuildSummary, BuildCreated)
export interface BuildResponse {
  id: string
  ownerName: string
  isMine: boolean
  updatedAt: string
  build: Build
}

export interface BuildSummary {
  id: string
  name: string
  championId: string
  updatedAt: string
}

interface BuildCreated {
  id: string
}

export const fetchBuild = (signal: AbortSignal, id: string) => getJson<BuildResponse>(`builds/${encodeURIComponent(id)}`, signal)

export const fetchMyBuilds = (signal: AbortSignal) => getJson<BuildSummary[]>('me/builds', signal)

// Everyone's builds of one champion, newest first (Web.Api ChampionBuildsFunction)
export const fetchChampionBuilds = (signal: AbortSignal, championId: string) =>
  getJson<RecentBuild[]>(`champions/${encodeURIComponent(championId)}/builds`, signal)

export async function saveBuild(build: Build, id?: string): Promise<string> {
  const saved = id
    ? await sendJson<BuildCreated>('PUT', `builds/${encodeURIComponent(id)}`, build)
    : await sendJson<BuildCreated>('POST', 'builds', build)
  return saved.id
}

export const deleteBuild = (id: string) => sendJson<void>('DELETE', `builds/${encodeURIComponent(id)}`)
