import type { Champion } from '../../api/champions'

// Ignores case, accents and punctuation, so "kaisa" finds Kai'Sa
const normalize = (text: string) =>
  text
    .normalize('NFD')
    .replace(/\p{Diacritic}/gu, '')
    .replace(/[^\p{Letter}\p{Number}]/gu, '')
    .toLowerCase()

export function filterChampions(champions: Champion[], query: string): Champion[] {
  const needle = normalize(query)
  if (!needle) return champions
  return champions.filter((c) => normalize(c.name).includes(needle))
}
