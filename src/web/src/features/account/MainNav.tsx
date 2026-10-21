import { NavLink } from 'react-router'
import { useCurrentUser } from './useCurrentUser'

export function MainNav() {
  const { state } = useCurrentUser()

  return (
    <nav className="main-nav">
      <NavLink to="/" end>
        Start
      </NavLink>
      <NavLink to="/buildy">Buildy</NavLink>
      {state.status === 'loggedIn' && <NavLink to="/statystyki">Statystyki</NavLink>}
    </nav>
  )
}
