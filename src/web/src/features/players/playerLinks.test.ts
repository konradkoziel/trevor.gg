import { describe, expect, it } from 'vitest'
import { playerPath, splitRiotId } from './playerLinks'

describe('splitRiotId', () => {
  it('splits on the last #', () => {
    expect(splitRiotId('Nick#EUNE')).toEqual({ name: 'Nick', tag: 'EUNE' })
  })

  it('rejects ids without a name or a tag', () => {
    expect(splitRiotId('Nick')).toBeNull()
    expect(splitRiotId('#EUNE')).toBeNull()
    expect(splitRiotId('Nick#')).toBeNull()
  })
})

describe('playerPath', () => {
  it('encodes spaces and accents', () => {
    expect(playerPath('Zły Gracz#EUNE')).toBe('/gracz/Z%C5%82y%20Gracz/EUNE')
  })

  it('is null for a malformed id', () => {
    expect(playerPath('Nick')).toBeNull()
  })
})
