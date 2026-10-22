export interface Box {
  left: number
  top: number
  width: number
  height: number
}

export interface Size {
  width: number
  height: number
}

// Space between the tooltip and what it describes, and between the tooltip and the window's edge
export const TooltipGap = 8

// Above the element and centred on it; below when there's no room above; always inside the window
export function tooltipPosition(anchor: Box, tooltip: Size, viewport: Size): { left: number; top: number } {
  const centred = anchor.left + anchor.width / 2 - tooltip.width / 2
  const left = clamp(centred, TooltipGap, viewport.width - tooltip.width - TooltipGap)

  const above = anchor.top - tooltip.height - TooltipGap
  const below = anchor.top + anchor.height + TooltipGap
  const top = above >= TooltipGap ? above : Math.min(below, viewport.height - tooltip.height - TooltipGap)

  return { left, top: Math.max(top, TooltipGap) }
}

const clamp = (value: number, min: number, max: number) => Math.max(min, Math.min(value, max))
