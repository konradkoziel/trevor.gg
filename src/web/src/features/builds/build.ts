import { emptyRunePage, type RunePage } from './runes'
import { emptySkillOrder, type SkillOrder } from './skillOrder'

// The six inventory slots
export const maxItems = 6
export const maxSummonerSpells = 2
export const maxBuildNameLength = 60

export interface Build {
  name: string
  championId: string
  // In buy order; the same item may appear twice (e.g. two Doran's Blades)
  items: string[]
  runes: RunePage
  summonerSpells: string[]
  skillOrder: SkillOrder
}

export const newBuild = (championId: string): Build => ({
  name: '',
  championId,
  items: [],
  runes: emptyRunePage(),
  summonerSpells: [],
  skillOrder: emptySkillOrder(),
})

export function addItem(build: Build, itemId: string): Build {
  return build.items.length >= maxItems ? build : { ...build, items: [...build.items, itemId] }
}

export function removeItem(build: Build, index: number): Build {
  return { ...build, items: build.items.filter((_, i) => i !== index) }
}

// offset -1 = one slot earlier, +1 = one slot later
export function moveItem(build: Build, index: number, offset: -1 | 1): Build {
  const target = index + offset
  if (target < 0 || target >= build.items.length) return build
  const items = [...build.items]
  ;[items[index], items[target]] = [items[target], items[index]]
  return { ...build, items }
}

export function toggleSummonerSpell(build: Build, spellId: string): Build {
  const spells = build.summonerSpells
  if (spells.includes(spellId)) return { ...build, summonerSpells: spells.filter((s) => s !== spellId) }
  // A third pick replaces the older one
  const kept = spells.length >= maxSummonerSpells ? spells.slice(1) : spells
  return { ...build, summonerSpells: [...kept, spellId] }
}

export const rename = (build: Build, name: string): Build => ({ ...build, name: name.slice(0, maxBuildNameLength) })
