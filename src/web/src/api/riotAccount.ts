import { getJson, sendJson } from './client'

// Mirror Web.Api.Features.RiotAccount (RiotAccountsView, RiotAccountView); id is the account's PUUID
export interface RiotAccountView {
  id: string
  riotId: string
  isMain: boolean
  // Games reported on Trevor's matches channel on Discord
  tracked: boolean
}

export interface RiotAccountsView {
  accounts: RiotAccountView[]
  maxAccounts: number
}

export const fetchRiotAccounts = (signal: AbortSignal) => getJson<RiotAccountsView>('me/riot', signal)

export const linkRiotAccount = (riotId: string) => sendJson<RiotAccountsView>('POST', 'me/riot', { riotId })

export const unlinkRiotAccount = (id: string) =>
  sendJson<RiotAccountsView>('DELETE', `me/riot/${encodeURIComponent(id)}`)

export const setRiotAccountTracked = (id: string, tracked: boolean) =>
  sendJson<RiotAccountsView>(tracked ? 'PUT' : 'DELETE', `me/riot/${encodeURIComponent(id)}/tracked`)

export const setMainRiotAccount = (id: string) =>
  sendJson<RiotAccountsView>('PUT', `me/riot/${encodeURIComponent(id)}/main`)
