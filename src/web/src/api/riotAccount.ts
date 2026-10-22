import { getJson, sendJson } from './client'

// Mirror Web.Api.Features.RiotAccount (RiotAccountView)
export interface RiotAccountView {
  linkedRiotId: string | null
}

export const fetchRiotAccount = (signal: AbortSignal) => getJson<RiotAccountView>('me/riot', signal)

export const linkRiotAccount = (riotId: string) => sendJson<RiotAccountView>('POST', 'me/riot', { riotId })

export const unlinkRiotAccount = () => sendJson<void>('DELETE', 'me/riot')
