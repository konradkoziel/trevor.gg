import type { Item } from '../../api/buildData'
import { normalizeSearch } from '../champions/filterChampions'

// Trinkets are free and every build has one
const hiddenTags = ['Trinket']

export function filterItems(items: Item[], query: string, finishedOnly: boolean): Item[] {
  const needle = normalizeSearch(query)
  return items.filter(
    (item) =>
      (!finishedOnly || item.isFinished) &&
      !item.tags.some((t) => hiddenTags.includes(t)) &&
      (!needle || normalizeSearch(item.name).includes(needle)),
  )
}
