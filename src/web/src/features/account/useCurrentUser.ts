import { useEffect, useState } from 'react'
import { fetchMe, type CurrentUser } from '../../api/me'
import { clearSession, getSessionToken } from '../../auth/session'

export type CurrentUserState =
  | { status: 'loading' }
  | { status: 'anonymous' }
  | { status: 'loggedIn'; user: CurrentUser }

export function useCurrentUser() {
  const [state, setState] = useState<CurrentUserState>(() =>
    getSessionToken() ? { status: 'loading' } : { status: 'anonymous' },
  )

  useEffect(() => {
    if (state.status !== 'loading') return
    const controller = new AbortController()
    fetchMe(controller.signal)
      .then((user) => setState({ status: 'loggedIn', user }))
      .catch(() => {
        if (!controller.signal.aborted) setState({ status: 'anonymous' })
      })
    return () => controller.abort()
  }, [state.status])

  const logout = () => {
    clearSession()
    setState({ status: 'anonymous' })
  }

  return { state, logout }
}
