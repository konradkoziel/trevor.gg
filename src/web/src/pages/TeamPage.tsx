import { LoginLink } from '../features/account/LoginLink'
import { hasSession, useCurrentUser } from '../features/account/useCurrentUser'
import { RecentBuilds } from '../features/home/RecentBuilds'
import { ShameLeaderboard } from '../features/home/ShameLeaderboard'

// /ekipa: everyone linked on the site, by wasted time, each name opening their profile; then their newest builds
export function TeamPage() {
  const { state } = useCurrentUser()

  if (!hasSession(state))
    return (
      <section className="panel">
        <p>Ekipa jest dla zalogowanych. Obcym Trevor nie pokazuje, kto ile przegrał.</p>
        <LoginLink />
      </section>
    )

  return (
    <>
      <ShameLeaderboard />
      <RecentBuilds />
    </>
  )
}
