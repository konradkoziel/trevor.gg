import { getJson } from './client'

// Mirrors Web.Api.Infrastructure.Sessions.SessionUser
export interface CurrentUser {
  discordId: string
  name: string
  avatarUrl: string
}

export const fetchMe = (signal?: AbortSignal) => getJson<CurrentUser>('me', signal)
