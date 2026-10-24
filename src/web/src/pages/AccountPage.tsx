import { LoginLink } from '../features/account/LoginLink'
import { useCurrentUser } from '../features/account/useCurrentUser'
import { RiotAccountPanel } from '../features/riotAccount/RiotAccountPanel'

export function AccountPage() {
  const { state } = useCurrentUser()

  if (state.status === 'anonymous')
    return (
      <section className="panel">
        <p>Najpierw się zaloguj, Trevor musi wiedzieć, kogo obrażać.</p>
        <LoginLink />
      </section>
    )

  return <RiotAccountPanel />
}
