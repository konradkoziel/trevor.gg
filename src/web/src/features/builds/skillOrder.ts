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

// The three abilities a max order ranks; the ultimate always goes at 6/11/16
export const basicSkills: SkillKey[] = ['Q', 'W', 'E']

// The whole 1-18 order from a max order like Q > E > W: one point in each basic ability first (in that order),
// the ultimate whenever it unlocks, otherwise the highest ability that may still go up. Gives the usual
// QEWQQRQEQEREEWWRWW for Q > E > W. Without all three abilities ranked, no order yet
export function orderFromPriority(priority: SkillKey[]): SkillOrder {
  const order = emptySkillOrder()
  if (!isFullPriority(priority)) return order

  for (let level = 1; level <= maxLevel; level++) {
    const learned = new Set(order.slice(0, level - 1))
    const candidates: SkillKey[] = [
      ...(ultimateLevels.includes(level) ? (['R'] as SkillKey[]) : []),
      ...priority.filter((key) => !learned.has(key)),
      ...priority,
    ]
    const key = candidates.find((k) => canAssign(order, level, k))
    if (key) order[level - 1] = key
  }
  return order
}

// The max order a saved build follows, as far as it can be told
export function priorityOf(order: SkillOrder): SkillKey[] {
  return completePriority(maxOrder(order))
}

// Clicking an ability ranks it next, clicking a ranked one takes it out; once two are ranked the third is obvious
export function togglePriority(priority: SkillKey[], key: SkillKey): SkillKey[] {
  if (priority.includes(key)) return priority.filter((k) => k !== key).slice(0, 1)
  return completePriority([...priority, key])
}

const isFullPriority = (priority: SkillKey[]) =>
  priority.length === basicSkills.length && basicSkills.every((k) => priority.includes(k))

function completePriority(priority: SkillKey[]): SkillKey[] {
  if (priority.length !== basicSkills.length - 1) return priority
  return [...priority, ...basicSkills.filter((k) => !priority.includes(k))]
}
