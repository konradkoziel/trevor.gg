import { cachedOnce, getJson } from './client'

// Mirrors Lol.Shared.Riot.Models.Champion
export interface Champion {
  id: string
  name: string
  title: string
  iconUrl: string
}

export const fetchChampions = cachedOnce(() => getJson<Champion[]>('champions'))
