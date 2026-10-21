import { apiUrl } from '../api/apiUrl'
import { parseSessionFragment, type LoginResult } from './sessionFragment'

// Where the session token is kept between visits; it expires on its own after 7 days (SessionTokens.Lifetime)
const storageKey = 'trevor.session'

export const loginUrl = `${apiUrl}/api/auth/discord/login`

// Called once at start-up: takes a token the login callback just sent and wipes it from the address bar
export function completeLogin(): LoginResult {
  const result = parseSessionFragment(window.location.hash)
  if (result.kind === 'none') return result

  history.replaceState(null, '', window.location.pathname + window.location.search)
  if (result.kind === 'session') write(result.token)
  return result
}

export function getSessionToken(): string | null {
  try {
    return localStorage.getItem(storageKey)
  } catch {
    return null
  }
}

export function clearSession() {
  try {
    localStorage.removeItem(storageKey)
  } catch {
    // Storage blocked: there was nothing to clear
  }
}

function write(token: string) {
  try {
    localStorage.setItem(storageKey, token)
  } catch {
    // Storage blocked (private mode): the user just stays logged out
  }
}
