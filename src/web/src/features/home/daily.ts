// The same pick for everyone all day, a different one tomorrow. Salt keeps two daily picks from moving in step
export function pickDaily<T>(items: readonly T[], date: Date, salt: string): T | undefined {
  if (items.length === 0) return undefined
  const day = `${date.getFullYear()}-${date.getMonth() + 1}-${date.getDate()}`
  return items[fnv1a(`${salt}:${day}`) % items.length]
}

// 32-bit FNV-1a: tiny, and consecutive days land far apart in the list
function fnv1a(text: string): number {
  let hash = 0x811c9dc5
  for (let i = 0; i < text.length; i++) {
    hash ^= text.charCodeAt(i)
    hash = Math.imul(hash, 0x01000193)
  }
  return hash >>> 0
}
