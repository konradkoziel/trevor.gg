import { describe, expect, it } from 'vitest'
import { addItem, maxItems, moveItem, newBuild, removeItem, rename, toggleSummonerSpell } from './build'

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

  it('keeps two summoner spells, the older giving way, toggling off on a second click', () => {
    let build = ['Flash', 'Ignite', 'Teleport'].reduce(toggleSummonerSpell, newBuild('Yasuo'))
    expect(build.summonerSpells).toEqual(['Ignite', 'Teleport'])
    build = toggleSummonerSpell(build, 'Ignite')
    expect(build.summonerSpells).toEqual(['Teleport'])
  })

  it('limits the name length', () => {
    expect(rename(newBuild('Yasuo'), 'x'.repeat(100)).name).toHaveLength(60)
  })
})
