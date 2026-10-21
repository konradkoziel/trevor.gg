const decimal = new Intl.NumberFormat('pl-PL', { maximumFractionDigits: 1 })
const whole = new Intl.NumberFormat('pl-PL', { maximumFractionDigits: 0 })

export const formatDecimal = (value: number) => decimal.format(value)

export const formatWhole = (value: number) => whole.format(value)

export const formatPercent = (value: number) => `${whole.format(value)}%`

// Colour bands for winrates, the way every stats site paints them
export function winRateTone(winRate: number): 'good' | 'bad' | 'neutral' {
  if (winRate >= 55) return 'good'
  if (winRate < 45) return 'bad'
  return 'neutral'
}
