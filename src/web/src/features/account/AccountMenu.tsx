import { Link } from 'react-router'
import { LoginLink } from './LoginLink'
import { useCurrentUser } from './useCurrentUser'

export function AccountMenu() {
  const { state, logout } = useCurrentUser()

  if (state.status === 'loading') return <span className="muted">…</span>
  if (state.status === 'anonymous') return <LoginLink />

  return (
    <div className="account">
      <Link to="/konto" className="account-link" title="Konto LoL">
        <img src={state.user.avatarUrl} alt="" width={32} height={32} />
        <span>{state.user.name}</span>
      </Link>
      <button type="button" className="button secondary" onClick={logout}>
        Wyloguj
      </button>
    </div>
  )
}
