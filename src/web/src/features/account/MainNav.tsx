import { NavLink } from 'react-router'
import { buildsPath } from '../builds/buildLinks'
import { mePath, teamPath } from './accountLinks'
import { hasSession, useCurrentUser } from './useCurrentUser'

// Start, the builds of every champion, the team, and your own things
export function MainNav() {
  const { state } = useCurrentUser()

  return (
    <nav className="main-nav">
      <NavLink to="/" end>
        Start
      </NavLink>
      <NavLink to={buildsPath}>Buildy</NavLink>
      {hasSession(state) && (
        <>
          <NavLink to={teamPath}>Ekipa</NavLink>
          <NavLink to={mePath}>Ja</NavLink>
        </>
      )}
    </nav>
  )
}
