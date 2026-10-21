// Which ability gets a point at each champion level (index 0 = level 1)
export type SkillKey = 'Q' | 'W' | 'E' | 'R'
export type SkillOrder = (SkillKey | null)[]

export const maxLevel = 18
export const skillKeys: SkillKey[] = ['Q', 'W', 'E', 'R']

const maxRank: Record<SkillKey, number> = { Q: 5, W: 5, E: 5, R: 3 }

// The ultimate unlocks its ranks at these levels
const ultimateLevels = [6, 11, 16]

export const emptySkillOrder = (): SkillOrder => Array<SkillKey | null>(maxLevel).fill(null)

// League's rules: a basic ability can have at most one rank per two levels (rank 3 needs level 5),
// the ultimate only at 6/11/16, and no ability above its max rank. Udyr-like exceptions are ignored.
export function isValidSkillOrder(order: SkillOrder): boolean {
  const ranks: Record<SkillKey, number> = { Q: 0, W: 0, E: 0, R: 0 }
  return order.every((key, index) => {
    if (key === null) return true
    const level = index + 1
    ranks[key]++
    if (ranks[key] > maxRank[key]) return false
    if (key === 'R') return ranks.R <= ultimateLevels.filter((l) => l <= level).length
    return ranks[key] <= Math.ceil(level / 2)
  })
}

export function canAssign(order: SkillOrder, level: number, key: SkillKey): boolean {
  return isValidSkillOrder(assign(order, level, key))
}

// Clicking the chosen cell again clears it
export function assign(order: SkillOrder, level: number, key: SkillKey): SkillOrder {
  const next = [...order]
  next[level - 1] = next[level - 1] === key ? null : key
  return next
}

// "Q > E > W": the order in which the basic abilities reach max rank, the way builds are usually summarised
export function maxOrder(order: SkillOrder): SkillKey[] {
  const ranks: Record<SkillKey, number> = { Q: 0, W: 0, E: 0, R: 0 }
  const maxed: SkillKey[] = []
  for (const key of order) {
    if (key === null || key === 'R') continue
    if (++ranks[key] === maxRank[key]) maxed.push(key)
  }
  return maxed
}
