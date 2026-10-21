import { describe, expect, it } from 'vitest'
import type { Champion } from '../../api/champions'
import { filterChampions } from './filterChampions'

const champion = (name: string): Champion => ({ id: name, name, title: '', iconUrl: '', key: 0, blurb: '', splashUrl: '' })
const champions = [champion('Ahri'), champion("Kai'Sa"), champion('Yasuo'), champion('Yone')]

describe('filterChampions', () => {
  it('returns everything for an empty query', () => {
    expect(filterChampions(champions, '  ')).toEqual(champions)
  })

  it('ignores case and punctuation', () => {
    expect(filterChampions(champions, 'KAISA').map((c) => c.name)).toEqual(["Kai'Sa"])
  })

  it('matches part of the name', () => {
    expect(filterChampions(champions, 'yo').map((c) => c.name)).toEqual(['Yone'])
    expect(filterChampions(champions, 'y').map((c) => c.name)).toEqual(['Yasuo', 'Yone'])
  })
})
