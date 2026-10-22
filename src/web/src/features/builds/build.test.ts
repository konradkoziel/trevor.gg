import { describe, expect, it } from 'vitest'
import {
  addItem,
  clearSummonerSpell,
  maxItems,
  moveItem,
  newBuild,
  removeItem,
  rename,
  setSummonerSpell,
} from './build'

describe('build', () => {
  it('holds at most six items, duplicates allowed', () => {
    let build = newBuild('Yasuo')
    for (let i = 0; i < maxItems + 2; i++) build = addItem(build, '1055')
    expect(build.items).toHaveLength(maxItems)
  })

  it('moves and removes items', () => {
    let build = ['a', 'b', 'c'].reduce(addItem, newBuild('Yasuo'))
    build = moveItem(build, 0, 1)
    expect(build.items).toEqual(['b', 'a', 'c'])
    expect(moveItem(build, 0, -1)).toBe(build)
    expect(removeItem(build, 1).items).toEqual(['b', 'c'])
  })

  it('fills two spell slots, swapping a spell picked for the other slot', () => {
    let build = setSummonerSpell(newBuild('Yasuo'), 1, 'Flash')
    expect(build.summonerSpells).toEqual(['Flash'])
    build = setSummonerSpell(build, 1, 'Ignite')
    expect(build.summonerSpells).toEqual(['Flash', 'Ignite'])
    build = setSummonerSpell(build, 0, 'Teleport')
    expect(build.summonerSpells).toEqual(['Teleport', 'Ignite'])
    build = setSummonerSpell(build, 0, 'Ignite')
    expect(build.summonerSpells).toEqual(['Ignite', 'Teleport'])
    expect(clearSummonerSpell(build, 0).summonerSpells).toEqual(['Teleport'])
  })

  it('limits the name length', () => {
    expect(rename(newBuild('Yasuo'), 'x'.repeat(100)).name).toHaveLength(60)
  })
})
