import { useEffect, useState, type ReactNode } from 'react'
import { fetchMe } from '../../api/me'
import { clearSession, getSessionToken } from '../../auth/session'
import { CurrentUserContext, type CurrentUserState } from './currentUserContext'

// One /api/me call for the whole app; the header, the editor and the home page all read it
export function CurrentUserProvider({ children }: { children: ReactNode }) {
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

  return <CurrentUserContext value={{ state, logout }}>{children}</CurrentUserContext>
}
