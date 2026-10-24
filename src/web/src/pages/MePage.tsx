import { Suspense } from 'react'
import { NavLink, Outlet } from 'react-router'
import { mePath, myAccountsPath, myBuildsPath } from '../features/account/accountLinks'
import { LoginLink } from '../features/account/LoginLink'
import { hasSession, useCurrentUser } from '../features/account/useCurrentUser'
import { Skeleton } from '../features/layout/Skeleton'

// /ja: everything of the logged-in user, a tab each: stats (/ja), builds (/ja/buildy), League accounts (/ja/konta)
export function MePage() {
  const { state } = useCurrentUser()

  if (!hasSession(state))
    return (
      <section className="panel">
        <p>
          To Twoja strona: statystyki, buildy i konta LoL. Najpierw się zaloguj, Trevor musi wiedzieć, kogo obrażać.
        </p>
        <LoginLink />
      </section>
    )

  return (
    <>
      <nav className="page-tabs" aria-label="Moje">
        <NavLink to={mePath} end>
          Statystyki
        </NavLink>
        <NavLink to={myBuildsPath}>Buildy</NavLink>
        <NavLink to={myAccountsPath}>Konta LoL</NavLink>
      </nav>
      {/* The tabs stay while the next tab's code downloads */}
      <Suspense fallback={<Skeleton rows={4} />}>
        <Outlet />
      </Suspense>
    </>
  )
}
