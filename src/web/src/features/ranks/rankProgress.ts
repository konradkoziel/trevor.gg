import type { RankedStanding } from '../../api/ranks'

// A division is won at 100 LP; Master and above have no divisions, so no bar
const LeaguePointsPerDivision = 100
const ApexTiers = ['MASTER', 'GRANDMASTER', 'CHALLENGER']

// Share of the way to the next division, 0-1, or null for apex tiers
export function divisionProgress(rank: RankedStanding): number | null {
  if (ApexTiers.includes(rank.tier)) return null
  return Math.min(Math.max(rank.leaguePoints, 0), LeaguePointsPerDivision) / LeaguePointsPerDivision
}
