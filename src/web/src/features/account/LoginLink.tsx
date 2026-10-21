import type { ReactNode } from 'react'
import { loginUrl, rememberReturnPath } from '../../auth/session'

export function LoginLink({ children = 'Zaloguj przez Discorda' }: { children?: ReactNode }) {
  return (
    <a className="button discord" href={loginUrl} onClick={rememberReturnPath}>
      {children}
    </a>
  )
}
