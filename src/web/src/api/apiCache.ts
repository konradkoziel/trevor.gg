// The last answer of every loader and argument, so a page visited again shows it at once while useApi asks again
type Key = string | null

// Plenty for one visit; the oldest answers go first (matches and profiles are opened one after another)
const MaxEntriesPerLoader = 50

const answers = new WeakMap<object, Map<Key, unknown>>()
let loaders: object[] = []

export function remember(loader: object, arg: Key, data: unknown) {
  let byArg = answers.get(loader)
  if (!byArg) {
    byArg = new Map()
    answers.set(loader, byArg)
    loaders.push(loader)
  }
  byArg.delete(arg)
  byArg.set(arg, data)
  if (byArg.size > MaxEntriesPerLoader) byArg.delete(byArg.keys().next().value as Key)
}

// Wrapped, so a remembered null or undefined still counts as an answer
export function recall<T>(loader: object, arg: Key): { data: T } | undefined {
  const byArg = answers.get(loader)
  return byArg?.has(arg) ? { data: byArg.get(arg) as T } : undefined
}

// On login and logout: most answers belong to the user
export function forgetAll() {
  for (const loader of loaders) answers.delete(loader)
  loaders = []
}
