import { describe, expect, it } from 'vitest'
import type { Item } from '../../api/buildData'
import { filterItems } from './filterItems'

const item = (name: string, isFinished: boolean, tags: string[] = []): Item => ({
  id: name,
  name,
  description: '',
  totalGold: 0,
  tags,
  isFinished,
  iconUrl: '',
})

const items = [item('Buty', false, ['Boots']), item('Nieskończona Krawędź', true), item('Totem', true, ['Trinket'])]

describe('filterItems', () => {
  it('shows only finished items unless asked otherwise, never trinkets', () => {
    expect(filterItems(items, '', true).map((i) => i.name)).toEqual(['Nieskończona Krawędź'])
    expect(filterItems(items, '', false).map((i) => i.name)).toEqual(['Buty', 'Nieskończona Krawędź'])
  })

  it('searches ignoring accents', () => {
    expect(filterItems(items, 'nieskonczona', true)).toHaveLength(1)
  })
})
