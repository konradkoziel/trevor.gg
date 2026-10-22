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

// Puts the spell in one of the two slots, like the game's client: a spell already in the other slot swaps places.
// Slots don't leave holes, so picking the second slot while the first is empty fills the first
export function setSummonerSpell(build: Build, slot: number, spellId: string): Build {
  const spells = [...build.summonerSpells]
  const current = spells.indexOf(spellId)
  if (current === slot) return build
  if (current >= 0) {
    if (slot >= spells.length) return build
    ;[spells[current], spells[slot]] = [spells[slot], spells[current]]
  } else if (slot < spells.length) spells[slot] = spellId
  else if (spells.length < maxSummonerSpells) spells.push(spellId)
  return { ...build, summonerSpells: spells }
}

export function clearSummonerSpell(build: Build, slot: number): Build {
  return { ...build, summonerSpells: build.summonerSpells.filter((_, i) => i !== slot) }
}

export const rename = (build: Build, name: string): Build => ({ ...build, name: name.slice(0, maxBuildNameLength) })
