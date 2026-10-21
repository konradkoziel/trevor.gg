import { describe, expect, it } from 'vitest'
import { pickDaily } from './daily'

const items = Array.from({ length: 50 }, (_, i) => i)

describe('pickDaily', () => {
  it('is the same all day', () => {
    expect(pickDaily(items, new Date(2026, 9, 21, 0, 5), 'a')).toBe(pickDaily(items, new Date(2026, 9, 21, 23, 55), 'a'))
  })

  it('changes from day to day', () => {
    const week = Array.from({ length: 7 }, (_, d) => pickDaily(items, new Date(2026, 9, 21 + d), 'a'))
    expect(new Set(week).size).toBeGreaterThan(4)
  })

  it('differs per salt', () => {
    const days = Array.from({ length: 7 }, (_, d) => new Date(2026, 9, 21 + d))
    expect(days.map((d) => pickDaily(items, d, 'a'))).not.toEqual(days.map((d) => pickDaily(items, d, 'b')))
  })

  it('handles an empty list', () => {
    expect(pickDaily([], new Date(), 'a')).toBeUndefined()
  })
})
