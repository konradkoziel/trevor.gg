import { getJson } from './client'

// Mirrors Lol.Shared.Riot.Models.Champion
export interface Champion {
  id: string
  name: string
  title: string
  iconUrl: string
}

export const fetchChampions = (signal?: AbortSignal) => getJson<Champion[]>('champions', signal)
