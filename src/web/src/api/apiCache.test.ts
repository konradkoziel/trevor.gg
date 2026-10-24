import { describe, expect, it } from 'vitest'
import { forgetAll, recall, remember } from './apiCache'

const loader = () => Promise.resolve(1)
const other = () => Promise.resolve(2)

describe('api cache', () => {
  it('remembers per loader and argument', () => {
    remember(loader, 'a', 1)
    remember(other, 'a', 2)

    expect(recall(loader, 'a')).toEqual({ data: 1 })
    expect(recall(other, 'a')).toEqual({ data: 2 })
    expect(recall(loader, 'b')).toBeUndefined()
  })

  it('keeps a remembered null apart from nothing', () => {
    remember(loader, null, null)

    expect(recall(loader, null)).toEqual({ data: null })
  })

  it('drops the oldest answers first', () => {
    for (let i = 0; i <= 50; i++) remember(loader, `m${i}`, i)

    expect(recall(loader, 'm0')).toBeUndefined()
    expect(recall(loader, 'm50')).toEqual({ data: 50 })
  })

  it('forgets everything on logout', () => {
    remember(loader, 'a', 1)
    forgetAll()

    expect(recall(loader, 'a')).toBeUndefined()
  })
})
