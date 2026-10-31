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

const SecondsPerMinute = 60

// Match length the way the client shows it, e.g. 31:05
export function formatDuration(seconds: number): string {
  const minutes = Math.floor(seconds / SecondsPerMinute)
  const rest = Math.floor(seconds % SecondsPerMinute)
  return `${minutes}:${String(rest).padStart(2, '0')}`
}

// (K + A) / D, a deathless game counting as one death, like StatsCalculator in Web.Api
export const kdaRatio = (kills: number, deaths: number, assists: number) => (kills + assists) / Math.max(deaths, 1)

// Wins out of all games in percent; 0 without games
export const winRateOf = (wins: number, losses: number) => (wins + losses === 0 ? 0 : (100 * wins) / (wins + losses))

const MinuteMs = 60_000
const HourMs = 60 * MinuteMs
const DayMs = 24 * HourMs

// "5 min temu", "3 h temu", "2 dni temu": when the last game was, without a date to read
export function formatAgo(at: Date, now: Date): string {
  const ms = Math.max(now.getTime() - at.getTime(), 0)
  if (ms < HourMs) return `${Math.max(Math.floor(ms / MinuteMs), 1)} min temu`
  if (ms < DayMs) return `${Math.floor(ms / HourMs)} h temu`
  const days = Math.floor(ms / DayMs)
  return days === 1 ? 'wczoraj' : `${days} dni temu`
}

const thousandsFormat = new Intl.NumberFormat('pl-PL', { minimumFractionDigits: 1, maximumFractionDigits: 1 })

// Gold the way the client shows it: 12,4k
export const formatThousands = (value: number) => `${thousandsFormat.format(value / 1000)}k`

// Polish noun after a number: 1 gra, 2 gry, 5 gier, 22 gry, 12 gier
export function plural(count: number, one: string, few: string, many: string): string {
  if (count === 1) return one
  const lastDigit = count % 10
  const lastTwo = count % 100
  return lastDigit >= 2 && lastDigit <= 4 && (lastTwo < 12 || lastTwo > 14) ? few : many
}
