import { ApiRequestError, UnauthorizedError } from './client'

// What to show the user when a call fails
export function errorText(error: unknown, fallback: string): string {
  if (error instanceof UnauthorizedError) return 'Sesja wygasła. Zaloguj się jeszcze raz.'
  if (error instanceof ApiRequestError) return error.message
  return fallback
}
