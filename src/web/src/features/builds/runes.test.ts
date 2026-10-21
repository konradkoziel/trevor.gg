import { describe, expect, it } from 'vitest'
import {
  emptyRunePage,
  isRunePageComplete,
  selectPrimaryRune,
  selectPrimaryTree,
  selectSecondaryRune,
  selectSecondaryTree,
} from './runes'

const precision = 8000
const domination = 8100
const sorcery = 8200

describe('rune page', () => {
  it('resets the primary runes when the primary tree changes', () => {
    let page = selectPrimaryTree(emptyRunePage(), precision)
    page = selectPrimaryRune(page, 0, 8005)
    page = selectPrimaryTree(page, domination)
    expect(page.primary).toEqual([null, null, null, null])
  })

  it('drops the secondary tree when it becomes the primary one', () => {
    let page = selectPrimaryTree(emptyRunePage(), precision)
    page = selectSecondaryTree(page, domination)
    page = selectSecondaryRune(page, 1, 8126)
    page = selectPrimaryTree(page, domination)
    expect(page.secondaryTreeId).toBeNull()
    expect(page.secondary).toEqual([])
  })

  it('does not allow the primary tree as the secondary one', () => {
    const page = selectSecondaryTree(selectPrimaryTree(emptyRunePage(), precision), precision)
    expect(page.secondaryTreeId).toBeNull()
  })

  it('keeps one rune per row in the primary tree, toggling on a second click', () => {
    let page = selectPrimaryRune(emptyRunePage(), 1, 9101)
    page = selectPrimaryRune(page, 1, 9111)
    expect(page.primary[1]).toBe(9111)
    expect(selectPrimaryRune(page, 1, 9111).primary[1]).toBeNull()
  })

  it('keeps two secondary runes from different rows, the oldest giving way', () => {
    let page = selectSecondaryTree(selectPrimaryTree(emptyRunePage(), precision), sorcery)
    page = selectSecondaryRune(page, 1, 8224)
    page = selectSecondaryRune(page, 2, 8210)
    page = selectSecondaryRune(page, 3, 8237)
    expect(page.secondary.map((r) => r.runeId)).toEqual([8210, 8237])

    // Same row: replaces that row's rune
    page = selectSecondaryRune(page, 3, 8236)
    expect(page.secondary.map((r) => r.runeId)).toEqual([8210, 8236])
  })

  it('never takes a keystone from the secondary tree', () => {
    const page = selectSecondaryRune(emptyRunePage(), 0, 8214)
    expect(page.secondary).toEqual([])
  })

  it('is complete with 4 primary and 2 secondary runes', () => {
    let page = selectPrimaryTree(emptyRunePage(), precision)
    ;[8005, 9101, 9104, 8014].forEach((id, row) => (page = selectPrimaryRune(page, row, id)))
    page = selectSecondaryTree(page, sorcery)
    expect(isRunePageComplete(page)).toBe(false)
    page = selectSecondaryRune(selectSecondaryRune(page, 1, 8224), 2, 8210)
    expect(isRunePageComplete(page)).toBe(true)
  })
})
