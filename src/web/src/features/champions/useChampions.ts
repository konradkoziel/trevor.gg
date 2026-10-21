import { useEffect, useState } from 'react'
import { fetchChampions, type Champion } from '../../api/champions'

export type ChampionsState =
  | { status: 'loading' }
  | { status: 'error' }
  | { status: 'ready'; champions: Champion[] }

export function useChampions(): ChampionsState {
  const [state, setState] = useState<ChampionsState>({ status: 'loading' })

  useEffect(() => {
    const controller = new AbortController()
    fetchChampions(controller.signal)
      .then((champions) => setState({ status: 'ready', champions }))
      .catch(() => {
        if (!controller.signal.aborted) setState({ status: 'error' })
      })
    return () => controller.abort()
  }, [])

  return state
}
