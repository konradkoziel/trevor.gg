import { describe, expect, it } from 'vitest'
import { assign, canAssign, emptySkillOrder, isValidSkillOrder, maxOrder, type SkillKey } from './skillOrder'

const order = (keys: string) => {
  const result = emptySkillOrder()
  ;[...keys].forEach((k, i) => (result[i] = k === '-' ? null : (k as SkillKey)))
  return result
}

describe('skill order rules', () => {
  it('accepts a normal Q max', () => {
    expect(isValidSkillOrder(order('QEWQQRQEQEREEWWRWW'))).toBe(true)
  })

  it('allows the ultimate only from level 6, 11 and 16', () => {
    expect(canAssign(emptySkillOrder(), 5, 'R')).toBe(false)
    expect(canAssign(emptySkillOrder(), 6, 'R')).toBe(true)
    expect(canAssign(order('QWEQQR'), 7, 'R')).toBe(false)
    expect(canAssign(order('QWEQQRQEQE'), 11, 'R')).toBe(true)
  })

  it('needs two levels per rank of a basic ability', () => {
    expect(canAssign(order('Q'), 2, 'Q')).toBe(false)
    expect(canAssign(order('QW'), 3, 'Q')).toBe(true)
    expect(canAssign(order('QWQ'), 4, 'Q')).toBe(false)
  })

  it('caps basic abilities at rank 5 and the ultimate at 3', () => {
    expect(isValidSkillOrder(order('QWQEQRQWQ'))).toBe(true)
    expect(canAssign(order('QWQEQRQWQ'), 10, 'Q')).toBe(false)
  })

  it('checks later levels too when an early one changes', () => {
    // Q at 1 and 3 is fine; adding Q at 2 breaks level 3 as well
    expect(canAssign(order('-WQ'), 1, 'Q')).toBe(true)
    expect(canAssign(order('Q-Q'), 2, 'Q')).toBe(false)
  })

  it('clears a cell clicked twice', () => {
    expect(assign(order('Q'), 1, 'Q')[0]).toBeNull()
  })

  it('summarises the max order', () => {
    expect(maxOrder(order('QEWQQRQEQEREEWWRWW'))).toEqual(['Q', 'E', 'W'])
    expect(maxOrder(order('QWE'))).toEqual([])
  })
})
