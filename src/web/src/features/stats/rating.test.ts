import { describe, expect, it } from 'vitest'
import { formatRating, ratingTone } from './rating'

describe('rating tone', () => {
  it('colours ratings clearly above or below an average teammate', () => {
    expect(ratingTone(1.4)).toBe('good')
    expect(ratingTone(1.0)).toBe('neutral')
    expect(ratingTone(0.5)).toBe('bad')
  })

  it('always shows one decimal', () => {
    expect(formatRating(1)).toBe('1,0')
    expect(formatRating(1.45)).toBe('1,5')
  })
})
