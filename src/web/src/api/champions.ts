import { cachedOnce, getJson } from './client'

// Mirrors Lol.Shared.Riot.Models.Champion
export interface Champion {
  id: string
  name: string
  title: string
  iconUrl: string
  // Riot's numeric champion key, the start of the lore, default skin art
  key: number
  blurb: string
  splashUrl: string
}

export const fetchChampions = cachedOnce(() => getJson<Champion[]>('champions'))
