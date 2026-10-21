import { clearSession, getSessionToken } from '../auth/session'
import { apiUrl } from './apiUrl'

// The session token is missing, forged or expired
export class UnauthorizedError extends Error {}

// A 4xx with a message meant for the user ({ "error": "..." } from Web.Api ApiError)
export class ApiRequestError extends Error {
  readonly status: number

  constructor(status: number, message: string) {
    super(message)
    this.status = status
  }
}

type Method = 'GET' | 'POST' | 'PUT' | 'DELETE'

async function request<T>(method: Method, path: string, body?: unknown, signal?: AbortSignal): Promise<T> {
  const token = getSessionToken()
  const headers: Record<string, string> = {}
  if (token) headers.Authorization = `Bearer ${token}`
  if (body !== undefined) headers['Content-Type'] = 'application/json'

  const response = await fetch(`${apiUrl}/api/${path}`, {
    method,
    signal,
    headers,
    body: body === undefined ? undefined : JSON.stringify(body),
  })
  if (response.status === 401) {
    clearSession()
    throw new UnauthorizedError()
  }
  if (!response.ok) throw new ApiRequestError(response.status, await errorMessage(response))
  if (response.status === 204) return undefined as T
  return (await response.json()) as T
}

async function errorMessage(response: Response): Promise<string> {
  try {
    const body = (await response.json()) as { error?: string }
    if (body.error) return body.error
  } catch {
    // No JSON body
  }
  return `Serwer odpowiedział ${response.status}. Spróbuj jeszcze raz za chwilę.`
}

export const getJson = <T>(path: string, signal?: AbortSignal) => request<T>('GET', path, undefined, signal)

export const sendJson = <T>(method: Exclude<Method, 'GET'>, path: string, body?: unknown) => request<T>(method, path, body)

// For data that is the same for every page of the visit (champions, items): one request, shared, retried after a failure
export function cachedOnce<T>(load: () => Promise<T>): () => Promise<T> {
  let pending: Promise<T> | undefined
  return () =>
    (pending ??= load().catch((error: unknown) => {
      pending = undefined
      throw error
    }))
}
