import { NavLink } from 'react-router'
import { hasSession, useCurrentUser } from './useCurrentUser'

export function MainNav() {
  const { state } = useCurrentUser()

  return (
    <nav className="main-nav">
      <NavLink to="/" end>
        Start
      </NavLink>
      <NavLink to="/buildy">Buildy</NavLink>
      {hasSession(state) && <NavLink to="/statystyki">Statystyki</NavLink>}
    </nav>
  )
}
