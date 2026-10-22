import type { MatchTeam } from '../../api/matches'

// Riot's id of the blue side
const BlueTeam = 100

// Arena pairs are headed by their place, the two sides of every other mode by the result
export const teamTitle = (team: MatchTeam) =>
  team.placement ? `${team.placement}. miejsce` : team.win ? 'Wygrana' : 'Przegrana'

export function teamSubtitle(team: MatchTeam): string {
  if (team.placement) return `Para ${team.teamId}`
  return team.teamId === BlueTeam ? 'Niebieska drużyna' : 'Czerwona drużyna'
}
