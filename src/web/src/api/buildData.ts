import { cachedOnce, getJson } from './client'

// Mirror Lol.Shared.Riot.Models (Item, Rune, RuneTree, SummonerSpell, ChampionAbility) and Web.Api BuildData
export interface Item {
  id: string
  name: string
  description: string
  totalGold: number
  tags: string[]
  isFinished: boolean
  iconUrl: string
}

export interface Rune {
  id: number
  name: string
  description: string
  iconUrl: string
}

export interface RuneTree {
  id: number
  name: string
  iconUrl: string
  // slots[0] = keystones, slots[1..3] = minor runes
  slots: Rune[][]
}

export interface SummonerSpell {
  id: string
  name: string
  description: string
  iconUrl: string
}

export interface BuildData {
  items: Item[]
  runeTrees: RuneTree[]
  summonerSpells: SummonerSpell[]
}

export type AbilityKey = 'P' | 'Q' | 'W' | 'E' | 'R'

export interface ChampionAbility {
  key: AbilityKey
  name: string
  description: string
  iconUrl: string
}

export const fetchBuildData = cachedOnce(() => getJson<BuildData>('build-data'))

export const fetchAbilities = (championId: string, signal?: AbortSignal) =>
  getJson<ChampionAbility[]>(`champions/${encodeURIComponent(championId)}/abilities`, signal)
