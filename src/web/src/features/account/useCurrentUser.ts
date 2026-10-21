import { useContext } from 'react'
import { CurrentUserContext } from './currentUserContext'

export const useCurrentUser = () => useContext(CurrentUserContext)
