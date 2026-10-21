import { createContext } from 'react'
import type { CurrentUser } from '../../api/me'

export type CurrentUserState =
  | { status: 'loading' }
  | { status: 'anonymous' }
  | { status: 'loggedIn'; user: CurrentUser }

export interface CurrentUserValue {
  state: CurrentUserState
  logout: () => void
}

export const CurrentUserContext = createContext<CurrentUserValue>({ state: { status: 'anonymous' }, logout: () => {} })
