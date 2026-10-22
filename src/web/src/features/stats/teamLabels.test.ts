import { describe, expect, it } from 'vitest'
import type { MatchTeam } from '../../api/matches'
import { teamSubtitle, teamTitle } from './teamLabels'

const team = (teamId: number, win: boolean, placement: number | null = null): MatchTeam => ({
  teamId,
  placement,
  win,
  kills: 0,
  deaths: 0,
  assists: 0,
  gold: 0,
  players: [],
})

describe('team labels', () => {
  it('names the two sides by result and colour', () => {
    expect(teamTitle(team(100, true))).toBe('Wygrana')
    expect(teamSubtitle(team(100, true))).toBe('Niebieska drużyna')
    expect(teamTitle(team(200, false))).toBe('Przegrana')
    expect(teamSubtitle(team(200, false))).toBe('Czerwona drużyna')
  })

  it('names Arena pairs by their place', () => {
    expect(teamTitle(team(3, true, 2))).toBe('2. miejsce')
    expect(teamSubtitle(team(3, true, 2))).toBe('Para 3')
  })
})
