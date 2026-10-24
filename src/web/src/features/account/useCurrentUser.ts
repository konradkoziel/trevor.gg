import { useContext } from 'react'
import { CurrentUserContext, type CurrentUserState } from './currentUserContext'

export const useCurrentUser = () => useContext(CurrentUserContext)

// A stored token, checked or still being checked: pages load their data alongside /api/me instead of after it
// (a token that turns out bad is a 401, which logs out)
export const hasSession = (state: CurrentUserState) => state.status !== 'anonymous'
