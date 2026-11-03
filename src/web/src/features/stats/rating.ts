import type { MatchAward } from '../../api/matches'

// Always one decimal, so a column of ratings lines up: 1,0 next to 1,4
const ratingFormat = new Intl.NumberFormat('pl-PL', { minimumFractionDigits: 1, maximumFractionDigits: 1 })

export const formatRating = (rating: number) => ratingFormat.format(rating)

// A fifth above or below an average teammate is worth a colour
const GoodRating = 1.2
const BadRating = 0.8

export function ratingTone(rating: number): 'good' | 'bad' | 'neutral' {
  if (rating >= GoodRating) return 'good'
  if (rating < BadRating) return 'bad'
  return 'neutral'
}

// The same marks as the bot's match reports
export const awardIcons: Record<MatchAward, { icon: string; label: string }> = {
  mvp: { icon: '🏆', label: 'MVP drużyny' },
  report: { icon: '🚨', label: 'Do reportu' },
}
