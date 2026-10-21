import { describe, expect, it } from 'vitest'
import { formatDecimal, formatPercent, winRateTone } from './format'

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
})
