import { Link } from 'react-router'
import { myAccountsPath } from './accountLinks'
import { LoginLink } from './LoginLink'
import { useCurrentUser } from './useCurrentUser'

export function AccountMenu() {
  const { state, logout } = useCurrentUser()

  if (state.status === 'loading') return <span className="account muted">…</span>
  if (state.status === 'anonymous')
    return (
      <div className="account">
        <LoginLink />
      </div>
    )

  return (
    <div className="account">
      <Link to={myAccountsPath} className="account-link" title="Konta LoL">
        <img src={state.user.avatarUrl} alt="" width={32} height={32} />
        <span>{state.user.name}</span>
      </Link>
      <button type="button" className="button secondary" onClick={logout}>
        Wyloguj
      </button>
    </div>
  )
}
