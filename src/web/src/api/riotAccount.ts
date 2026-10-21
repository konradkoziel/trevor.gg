import { getJson, sendJson } from './client'

// Mirror Web.Api.Features.RiotAccount (RiotAccountView, RiotChallengeView)
export interface RiotChallenge {
  riotId: string
  iconId: number
  iconUrl: string | null
  expiresAt: string
}

export interface RiotAccountView {
  linkedRiotId: string | null
  challenge: RiotChallenge | null
}

export const fetchRiotAccount = (signal: AbortSignal) => getJson<RiotAccountView>('me/riot', signal)

export const startRiotLink = (riotId: string) => sendJson<RiotChallenge>('POST', 'me/riot/challenge', { riotId })

export const verifyRiotLink = () => sendJson<RiotAccountView>('POST', 'me/riot/verify')

export const unlinkRiotAccount = () => sendJson<void>('DELETE', 'me/riot')
