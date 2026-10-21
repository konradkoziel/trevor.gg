// Runes of a build: keystone + one rune from each minor row of the primary tree,
// two runes from different minor rows of another (secondary) tree
export interface RunePage {
  primaryTreeId: number | null
  // index 0 = keystone, 1..3 = the minor rows
  primary: (number | null)[]
  secondaryTreeId: number | null
  // At most two, each from a different minor row; the oldest one gives way to a third pick
  secondary: { row: number; runeId: number }[]
}

export const minorRows = [1, 2, 3]
export const maxSecondaryRunes = 2

export const emptyRunePage = (): RunePage => ({
  primaryTreeId: null,
  primary: [null, null, null, null],
  secondaryTreeId: null,
  secondary: [],
})

export function selectPrimaryTree(page: RunePage, treeId: number): RunePage {
  if (page.primaryTreeId === treeId) return page
  // The secondary tree can't be the same one
  const secondaryClash = page.secondaryTreeId === treeId
  return {
    primaryTreeId: treeId,
    primary: [null, null, null, null],
    secondaryTreeId: secondaryClash ? null : page.secondaryTreeId,
    secondary: secondaryClash ? [] : page.secondary,
  }
}

export function selectPrimaryRune(page: RunePage, row: number, runeId: number): RunePage {
  const primary = [...page.primary]
  primary[row] = primary[row] === runeId ? null : runeId
  return { ...page, primary }
}

export function selectSecondaryTree(page: RunePage, treeId: number): RunePage {
  if (treeId === page.primaryTreeId || treeId === page.secondaryTreeId) return page
  return { ...page, secondaryTreeId: treeId, secondary: [] }
}

export function selectSecondaryRune(page: RunePage, row: number, runeId: number): RunePage {
  if (row === 0) return page // keystones only come from the primary tree
  const current = page.secondary
  if (current.some((r) => r.runeId === runeId)) return { ...page, secondary: current.filter((r) => r.runeId !== runeId) }

  const others = current.filter((r) => r.row !== row)
  const kept = others.length >= maxSecondaryRunes ? others.slice(1) : others
  return { ...page, secondary: [...kept, { row, runeId }] }
}

export const isRunePageComplete = (page: RunePage) =>
  page.primary.every((id) => id !== null) && page.secondary.length === maxSecondaryRunes
