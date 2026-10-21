import { useEffect, useState } from 'react'

export type ApiState<T> = { status: 'loading' } | { status: 'error'; error: unknown } | { status: 'ready'; data: T }

type Loader<T, A> = (signal: AbortSignal, arg: A) => Promise<T>

// Loads again whenever arg changes; pass a module-level load function so it stays the same between renders.
// A response for an arg the component already moved away from is ignored.
export function useApi<T, A extends string | null = null>(load: Loader<T, A>, arg: A = null as A): ApiState<T> {
  const [result, setResult] = useState<{ arg: A; state: ApiState<T> }>()

  useEffect(() => {
    const controller = new AbortController()
    load(controller.signal, arg)
      .then((data) => setResult({ arg, state: { status: 'ready', data } }))
      .catch((error: unknown) => {
        if (!controller.signal.aborted) setResult({ arg, state: { status: 'error', error } })
      })
    return () => controller.abort()
  }, [load, arg])

  return result && result.arg === arg ? result.state : { status: 'loading' }
}
