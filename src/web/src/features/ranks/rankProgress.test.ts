import { describe, expect, it } from 'vitest'
import type { RankedStanding } from '../../api/ranks'
import { divisionProgress } from './rankProgress'

const rank = (tier: string, leaguePoints: number): RankedStanding => ({
  queue: 'Solo/Duo',
  tier,
  label: tier,
  leaguePoints,
  wins: 1,
  losses: 1,
  emblemUrl: '',
})

describe('divisionProgress', () => {
  it('is LP out of 100 below Master', () => {
    expect(divisionProgress(rank('DIAMOND', 33))).toBe(0.33)
    expect(divisionProgress(rank('GOLD', 100))).toBe(1)
  })

  it('has no bar for apex tiers', () => {
    expect(divisionProgress(rank('MASTER', 250))).toBeNull()
  })
})
