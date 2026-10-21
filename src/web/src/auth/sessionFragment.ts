// What the API's login callback puts after # (src/Web.Api/Features/Auth/SiteRedirects.cs)
export type LoginErrorReason = 'failed' | 'not-member'

export type LoginResult =
  | { kind: 'session'; token: string }
  | { kind: 'error'; reason: LoginErrorReason }
  | { kind: 'none' }

const sessionPrefix = '#session='
const loginError = '#login-error'
const notMember = '#login-error=not-member'

export function parseSessionFragment(hash: string): LoginResult {
  if (hash.startsWith(sessionPrefix)) {
    const token = decodeURIComponent(hash.slice(sessionPrefix.length))
    return token ? { kind: 'session', token } : { kind: 'error', reason: 'failed' }
  }
  if (hash === notMember) return { kind: 'error', reason: 'not-member' }
  if (hash.startsWith(loginError)) return { kind: 'error', reason: 'failed' }
  return { kind: 'none' }
}
