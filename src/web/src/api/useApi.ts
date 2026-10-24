import { useEffect, useState } from 'react'
import { recall, remember } from './apiCache'

export type ApiState<T> = { status: 'loading' } | { status: 'error'; error: unknown } | { status: 'ready'; data: T }

type Loader<T, A> = (signal: AbortSignal, arg: A) => Promise<T>

// Loads again whenever arg changes; pass a module-level load function so it stays the same between renders.
// A response for an arg the component already moved away from is ignored.
// What the same loader answered before is shown at once and replaced by the fresh answer, so going back to a page
// doesn't load it from scratch; a failed refresh keeps the old answer on screen.
export function useApi<T, A extends string | null = null>(load: Loader<T, A>, arg: A = null as A): ApiState<T> {
  const [result, setResult] = useState<{ arg: A; state: ApiState<T> }>()

  useEffect(() => {
    const controller = new AbortController()
    load(controller.signal, arg)
      .then((data) => {
        remember(load, arg, data)
        setResult({ arg, state: { status: 'ready', data } })
      })
      .catch((error: unknown) => {
        if (!controller.signal.aborted) setResult({ arg, state: { status: 'error', error } })
      })
    return () => controller.abort()
  }, [load, arg])

  const fresh = result && result.arg === arg ? result.state : undefined
  if (fresh?.status === 'ready') return fresh
  const remembered = recall<T>(load, arg)
  if (remembered) return { status: 'ready', data: remembered.data }
  return fresh ?? { status: 'loading' }
}
