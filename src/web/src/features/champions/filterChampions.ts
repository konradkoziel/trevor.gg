import type { Champion } from '../../api/champions'

// Ignores case, accents and punctuation, so "kaisa" finds Kai'Sa
export const normalizeSearch = (text: string) =>
  text
    .normalize('NFD')
    .replace(/\p{Diacritic}/gu, '')
    .replace(/[^\p{Letter}\p{Number}]/gu, '')
    .toLowerCase()

export function filterChampions(champions: Champion[], query: string): Champion[] {
  const needle = normalizeSearch(query)
  if (!needle) return champions
  return champions.filter((c) => normalizeSearch(c.name).includes(needle))
}
