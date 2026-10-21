import { loginUrl } from '../../auth/session'
import { useCurrentUser } from './useCurrentUser'

export function AccountMenu() {
  const { state, logout } = useCurrentUser()

  if (state.status === 'loading') return <span className="muted">…</span>

  if (state.status === 'anonymous')
    return (
      <a className="button discord" href={loginUrl}>
        Zaloguj przez Discorda
      </a>
    )

  return (
    <div className="account">
      <img src={state.user.avatarUrl} alt="" width={32} height={32} />
      <span>{state.user.name}</span>
      <button type="button" className="button secondary" onClick={logout}>
        Wyloguj
      </button>
    </div>
  )
}
