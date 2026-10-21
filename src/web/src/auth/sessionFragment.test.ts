import { describe, expect, it } from 'vitest'
import { parseSessionFragment } from './sessionFragment'

describe('parseSessionFragment', () => {
  it('reads the token the login callback sends', () => {
    expect(parseSessionFragment('#session=eyJ.abc.def')).toEqual({ kind: 'session', token: 'eyJ.abc.def' })
  })

  it('decodes an escaped token', () => {
    expect(parseSessionFragment('#session=a%2Bb')).toEqual({ kind: 'session', token: 'a+b' })
  })

  it('reports a failed login', () => {
    expect(parseSessionFragment('#login-error')).toEqual({ kind: 'error' })
    expect(parseSessionFragment('#session=')).toEqual({ kind: 'error' })
  })

  it('ignores any other fragment', () => {
    expect(parseSessionFragment('')).toEqual({ kind: 'none' })
    expect(parseSessionFragment('#top')).toEqual({ kind: 'none' })
  })
})
