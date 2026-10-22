import { describe, expect, it } from 'vitest'
import { TooltipGap, tooltipPosition } from './tooltipPosition'

const viewport = { width: 1000, height: 800 }
const tooltip = { width: 200, height: 100 }

describe('tooltipPosition', () => {
  it('sits above the element, centred', () => {
    expect(tooltipPosition({ left: 400, top: 300, width: 40, height: 40 }, tooltip, viewport)).toEqual({
      left: 320,
      top: 300 - 100 - TooltipGap,
    })
  })

  it('goes below when there is no room above', () => {
    expect(tooltipPosition({ left: 400, top: 20, width: 40, height: 40 }, tooltip, viewport).top).toBe(
      20 + 40 + TooltipGap,
    )
  })

  it('stays inside the window at the edges', () => {
    expect(tooltipPosition({ left: 0, top: 300, width: 40, height: 40 }, tooltip, viewport).left).toBe(TooltipGap)
    expect(tooltipPosition({ left: 980, top: 300, width: 20, height: 40 }, tooltip, viewport).left).toBe(
      1000 - 200 - TooltipGap,
    )
  })
})
