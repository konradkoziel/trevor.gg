import { NavLink } from 'react-router'
import { useCurrentUser } from './useCurrentUser'

export function MainNav() {
  const { state } = useCurrentUser()
  if (state.status !== 'loggedIn') return null

  return (
    <nav className="main-nav">
      <NavLink to="/" end>
        Buildy
      </NavLink>
      <NavLink to="/statystyki">Statystyki</NavLink>
    </nav>
  )
}
