import { describe, expect, it } from 'vitest'
import type { RecentMatch } from '../../api/stats'
import { playerBadges } from './badges'

const game = (win: boolean, k = 5, d = 5, a = 5, championName = 'Ahri', queue = 'Ranked Solo/Duo'): RecentMatch => ({
  matchId: 'EUN1_1',
  championName,
  iconUrl: null,
  win,
  kills: k,
  deaths: d,
  assists: a,
  csPerMinute: 6,
  queue,
  playedAt: '2026-10-30T20:00:00Z',
  durationSeconds: 1800,
})

const labels = (games: RecentMatch[]) => playerBadges(games).map((b) => b.label)
const mixed = (champion: string, i: number) => game(i % 2 === 0, 5, 5, 5, champion)

describe('player badges', () => {
  it('names a streak from the newest game', () => {
    expect(labels([game(true), game(true), game(true), game(false)])).toEqual(['3 wygrane z rzędu'])
    const losses = ['A', 'B', 'C', 'D', 'E'].map((c) => game(false, 5, 5, 5, c))
    expect(labels([...losses, game(true)])).toEqual(['Tilt: 5 porażek z rzędu'])
    expect(labels([game(true), game(true), game(false)])).toEqual([])
  })

  it('finds the best deathless game and the worst feed', () => {
    const badges = playerBadges([game(true, 3, 0, 4), game(false, 1, 12, 2, 'Garen'), game(true, 8, 0, 9)])

    expect(badges.find((b) => b.label === 'Gra bez śmierci')?.detail).toBe('Ahri 8/0/9')
    expect(badges.find((b) => b.tone === 'bad')).toMatchObject({ label: 'Rekord: 12 śmierci w grze' })
  })

  it('spots ARAM players and one-tricks', () => {
    const aram = ['A', 'B', 'C', 'D', 'E'].map((c, i) => game(i % 2 === 0, 5, 5, 5, c, 'ARAM'))
    const oneTrick = [...Array(6)].map((_, i) => mixed(i < 5 ? 'Yasuo' : 'Ahri', i))

    expect(labels(aram)).toEqual(['Król ARAM-ów'])
    expect(labels(oneTrick)).toEqual(['One-trick: Yasuo'])
  })

  it('stays quiet about an ordinary week', () => {
    expect(labels(['A', 'B', 'C', 'D'].map(mixed))).toEqual([])
    expect(labels([])).toEqual([])
  })
})
