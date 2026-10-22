import { describe, expect, it } from 'vitest'
import {
  formatAgo,
  formatDecimal,
  formatDuration,
  formatThousands,
  formatPercent,
  kdaRatio,
  winRateOf,
  winRateTone,
} from './format'

describe('stats formatting', () => {
  it('uses Polish decimals', () => {
    expect(formatDecimal(3.14)).toBe('3,1')
    expect(formatPercent(66.7)).toBe('67%')
  })

  it('colours winrates', () => {
    expect(winRateTone(60)).toBe('good')
    expect(winRateTone(50)).toBe('neutral')
    expect(winRateTone(40)).toBe('bad')
  })

  it('formats match length as minutes and seconds', () => {
    expect(formatDuration(1865)).toBe('31:05')
    expect(formatDuration(59)).toBe('0:59')
  })

  it('counts a deathless game as one death', () => {
    expect(kdaRatio(8, 2, 4)).toBe(6)
    expect(kdaRatio(5, 0, 5)).toBe(10)
  })

  it('gives 0% winrate without games', () => {
    expect(winRateOf(63, 55)).toBeCloseTo(53.4, 1)
    expect(winRateOf(0, 0)).toBe(0)
  })

  it('says how long ago the last game was', () => {
    const now = new Date('2026-10-22T12:00:00Z')
    expect(formatAgo(new Date('2026-10-22T11:58:00Z'), now)).toBe('2 min temu')
    expect(formatAgo(new Date('2026-10-22T09:00:00Z'), now)).toBe('3 h temu')
    expect(formatAgo(new Date('2026-10-21T09:00:00Z'), now)).toBe('wczoraj')
    expect(formatAgo(new Date('2026-10-17T12:00:00Z'), now)).toBe('5 dni temu')
  })

  it('shows gold in thousands', () => {
    expect(formatThousands(12400)).toBe('12,4k')
    expect(formatThousands(900)).toBe('0,9k')
  })
})
