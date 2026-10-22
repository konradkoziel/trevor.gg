import { formatPercent, winRateTone } from './format'

// Circle size in SVG units; the CSS sets the real size
const Size = 64
const Stroke = 7
const Radius = (Size - Stroke) / 2
const Circumference = 2 * Math.PI * Radius

export function WinRateRing({ winRate }: { winRate: number }) {
  const won = (Math.min(Math.max(winRate, 0), 100) / 100) * Circumference
  return (
    <svg
      className={`winrate-ring ${winRateTone(winRate)}`}
      viewBox={`0 0 ${Size} ${Size}`}
      role="img"
      aria-label={`Winrate ${formatPercent(winRate)}`}
    >
      <circle className="ring-track" cx={Size / 2} cy={Size / 2} r={Radius} strokeWidth={Stroke} />
      <circle
        className="ring-value"
        cx={Size / 2}
        cy={Size / 2}
        r={Radius}
        strokeWidth={Stroke}
        strokeDasharray={`${won} ${Circumference}`}
        transform={`rotate(-90 ${Size / 2} ${Size / 2})`}
      />
      <text x="50%" y="50%" dominantBaseline="central" textAnchor="middle">
        {formatPercent(winRate)}
      </text>
    </svg>
  )
}
