import { clearSession, getSessionToken } from '../auth/session'
import { apiUrl } from './apiUrl'

// The session token is missing, forged or expired
export class UnauthorizedError extends Error {}

export async function getJson<T>(path: string, signal?: AbortSignal): Promise<T> {
  const token = getSessionToken()
  const response = await fetch(`${apiUrl}/api/${path}`, {
    signal,
    headers: token ? { Authorization: `Bearer ${token}` } : undefined,
  })
  if (response.status === 401) {
    clearSession()
    throw new UnauthorizedError()
  }
  if (!response.ok) throw new Error(`GET /api/${path} failed: ${response.status}`)
  return (await response.json()) as T
}
