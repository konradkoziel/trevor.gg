// What the API's login callback puts after # (src/Web.Api/Features/Auth/SiteRedirects.cs)
export type LoginResult = { kind: 'session'; token: string } | { kind: 'error' } | { kind: 'none' }

const sessionPrefix = '#session='
const loginError = '#login-error'

export function parseSessionFragment(hash: string): LoginResult {
  if (hash.startsWith(sessionPrefix)) {
    const token = decodeURIComponent(hash.slice(sessionPrefix.length))
    return token ? { kind: 'session', token } : { kind: 'error' }
  }
  if (hash === loginError) return { kind: 'error' }
  return { kind: 'none' }
}
