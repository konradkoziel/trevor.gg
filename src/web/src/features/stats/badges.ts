import type { RecentMatch } from '../../api/stats'
import { formatDecimal, kdaRatio, plural } from './format'

// Trevor's comments on a player's recent games, worked out from the numbers, no AI
export interface PlayerBadge {
  label: string
  tone: 'good' | 'bad' | 'neutral'
  // The game or count behind it, for the tooltip
  detail: string
}

// Three in a row is a streak; two is just an evening
const StreakMin = 3
// Deaths in one game that earn a mention
const DeathRecordMin = 10
// A deathless game counts only with something done in it
const PerfectMinTakedowns = 5
// Mostly ARAM, or mostly one champion, over enough games to mean it
const MostlyShare = 0.5
const AramMinGames = 4
const OneTrickMinGames = 5
// More stops reading like badges and starts reading like a report
const MaxBadges = 4

// The queue name RiotNames.Queue in Lol.Shared gives ARAM
const AramQueue = 'ARAM'

// recent: newest first, as /api/me/stats and the profiles send them
export function playerBadges(recent: RecentMatch[]): PlayerBadge[] {
  return [streak(recent), perfectGame(recent), deathRecord(recent), aramKing(recent), oneTrick(recent)]
    .filter((b): b is PlayerBadge => b !== null)
    .slice(0, MaxBadges)
}

function streak(recent: RecentMatch[]): PlayerBadge | null {
  if (recent.length === 0) return null
  const win = recent[0].win
  const length = recent.findIndex((m) => m.win !== win)
  const n = length === -1 ? recent.length : length
  if (n < StreakMin) return null
  return win
    ? { label: `${n} ${plural(n, 'wygrana', 'wygrane', 'wygranych')} z rzędu`, tone: 'good', detail: 'Ostatnie gry' }
    : { label: `Tilt: ${n} ${plural(n, 'porażka', 'porażki', 'porażek')} z rzędu`, tone: 'bad', detail: 'Ostatnie gry' }
}

function perfectGame(recent: RecentMatch[]): PlayerBadge | null {
  const game = recent
    .filter((m) => m.deaths === 0 && m.kills + m.assists >= PerfectMinTakedowns)
    .sort((a, b) => b.kills + b.assists - (a.kills + a.assists))[0]
  return game
    ? { label: 'Gra bez śmierci', tone: 'good', detail: `${game.championName} ${game.kills}/0/${game.assists}` }
    : null
}

function deathRecord(recent: RecentMatch[]): PlayerBadge | null {
  const game = [...recent].sort((a, b) => b.deaths - a.deaths)[0]
  return game && game.deaths >= DeathRecordMin
    ? {
        label: `Rekord: ${game.deaths} śmierci w grze`,
        tone: 'bad',
        detail: `${game.championName} ${game.kills}/${game.deaths}/${game.assists}, ${formatDecimal(kdaRatio(game.kills, game.deaths, game.assists))} KDA`,
      }
    : null
}

function aramKing(recent: RecentMatch[]): PlayerBadge | null {
  const aram = recent.filter((m) => m.queue === AramQueue).length
  return aram >= AramMinGames && aram / recent.length >= MostlyShare
    ? { label: 'Król ARAM-ów', tone: 'neutral', detail: `${aram} z ${recent.length} ostatnich gier` }
    : null
}

function oneTrick(recent: RecentMatch[]): PlayerBadge | null {
  const counts = new Map<string, number>()
  for (const m of recent) counts.set(m.championName, (counts.get(m.championName) ?? 0) + 1)
  const [champion, games] = [...counts].sort((a, b) => b[1] - a[1])[0] ?? ['', 0]
  return games >= OneTrickMinGames && games / recent.length >= MostlyShare
    ? { label: `One-trick: ${champion}`, tone: 'neutral', detail: `${games} z ${recent.length} ostatnich gier` }
    : null
}
