// Mirrors Web.Api.Features.Ranks.RankedStanding
export interface RankedStanding {
  queue: string
  tier: string
  label: string
  leaguePoints: number
  wins: number
  losses: number
  emblemUrl: string
}

// Queue names as RankedStandings in Web.Api sends them
export const SoloQueue = 'Solo/Duo'
export const FlexQueue = 'Flex'
