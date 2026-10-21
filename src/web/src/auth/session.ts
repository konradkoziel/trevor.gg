import { apiUrl } from '../api/apiUrl'
import { parseSessionFragment, type LoginResult } from './sessionFragment'

// Where the session token is kept between visits; it expires on its own after 7 days (SessionTokens.Lifetime)
const storageKey = 'trevor.session'

// The page to come back to after Discord, so logging in on a shared build doesn't drop you on the home page
const returnPathKey = 'trevor.returnPath'

export const loginUrl = `${apiUrl}/api/auth/discord/login`

export function rememberReturnPath() {
  safely(() => sessionStorage.setItem(returnPathKey, window.location.pathname))
}

// Called once at start-up: takes a token the login callback just sent and wipes it from the address bar
export function completeLogin(): LoginResult {
  const result = parseSessionFragment(window.location.hash)
  if (result.kind === 'none') return result

  const returnPath = safely(() => sessionStorage.getItem(returnPathKey)) ?? null
  safely(() => sessionStorage.removeItem(returnPathKey))
  // Only paths of this site, never another origin
  const path = returnPath?.startsWith('/') && !returnPath.startsWith('//') ? returnPath : window.location.pathname
  history.replaceState(null, '', path)

  if (result.kind === 'session') safely(() => localStorage.setItem(storageKey, result.token))
  return result
}

export function getSessionToken(): string | null {
  return safely(() => localStorage.getItem(storageKey)) ?? null
}

export function clearSession() {
  safely(() => localStorage.removeItem(storageKey))
}

// Storage can be blocked (private mode, strict settings); the site then just behaves as logged out
function safely<T>(action: () => T): T | undefined {
  try {
    return action()
  } catch {
    return undefined
  }
}
